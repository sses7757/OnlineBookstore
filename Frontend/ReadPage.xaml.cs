using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReadPage : Page, IRefreshAdminInterface
    {
        public ReadPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.bulletScreen = new BulletScreen();
            this.bulletPool = new List<LiveComment>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var (key, url, bookId) = ((string key, string url, int bookId))e.Parameter;
            if (this.bookId == bookId)
                return;
            this.privateKey = key;
            this.bookId = bookId;
            this.Source = new Uri(url);
            this.LoadAsync();
        }

        private int bookId = -1;
        private string privateKey;
        internal Uri Source { private set; get; }
        internal ObservableCollection<BitmapImage> PdfPages {
            get;
            private set;
        } = new ObservableCollection<BitmapImage>();


        private async void LoadAsync()
        {
            if (Source == null)
            {
                PdfPages.Clear();
            }
            else
            {
                PdfDocument doc;
                if (Source.IsFile || !Source.IsWebUri())
                {
                    doc = await LoadFromLocalAsync();
                }
                else if (Source.IsWebUri())
                {
                    doc = await LoadFromRemoteAsync();
                }
                else
                {
                    Debug.WriteLine($"Source '{Source.ToString()}' could not be recognized!");
                    return;
                }
                LoadBulletsAsync(doc.PageCount);
            }
        }

        private async void LoadBulletsAsync(uint maxPageNum)
        {
            for (uint i = 1; i <= maxPageNum; ++i)
            {
                var collection = new DanmuCollection(this.bookId, i);
                await collection.Reload();
                this.bulletPool.AddRange(collection.Danmus.Select(d => new LiveComment(d, i)));
            }
        }

        private async Task<PdfDocument> LoadFromRemoteAsync()
        {
            MemoryStream memStream;
            while (true)
            {
                try
                {
                    HttpClient client = new HttpClient();
                    var stream = await client.GetStreamAsync(Source);
                    memStream = new MemoryStream();
                    await stream.CopyToAsync(memStream);
                }
                catch (Exception)
                {
                    continue;
                }
                break;
            }
            memStream.Position = 0;
            var doc = await PdfDocument.LoadFromStreamAsync(
                                memStream.AsRandomAccessStream(),
                                privateKey);

            Load(doc);
            return doc;
        }

        private async Task<PdfDocument> LoadFromLocalAsync()
        {
            var f = await StorageFile.GetFileFromApplicationUriAsync(Source);
            var doc = await PdfDocument.LoadFromFileAsync(f, privateKey);

            Load(doc);
            return doc;
        }

        private async void Load(PdfDocument pdfDoc)
        {
            PdfPages.Clear();
            this.LoadingControl(pdfDoc.PageCount);
            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                BitmapImage image = new BitmapImage();

                var page = pdfDoc.GetPage(i);

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }

                PdfPages.Add(image);
            }
        }

        private async void LoadingControl(uint pageCount, uint preloadCount = 10)
        {
            loadingControl.IsLoading = true;
            while (PdfPages.Count < Math.Min(preloadCount, pageCount))
            {
                await Task.Delay(Util.REFRESH_RATE);
            }
            loadingControl.IsLoading = false;
        }

        public void RefreshButtonPressed()
        {
            this.LoadAsync();
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }

        private readonly BulletScreen bulletScreen;

        private readonly List<LiveComment> bulletPool;

        /// <summary>
        /// Load comments according to the playback position
        /// </summary>
        private void LoadComments(uint pagePosNow)
        {
            // Get all the avaliable bullets's id.
            List<Guid> idList = bulletScreen.Bullets.Where(o => !o.IsObsolete).
                                                     Select(o => o.CommentItem.ID).
                                                     ToList();
            // Get comments need to load to screen from the comment pool.
            IEnumerable<LiveComment> list = this.bulletPool.
                                            Where(o => o.PageNum == pagePosNow
                                                  && !idList.Contains(o.ID));
            // default positon is top right.
            Vector2 startPosition = new Vector2(1, 0);
            // If there's any comment need to load to screen
            if (list.Count() > 0)
            {
                // Animated control's actual size
                Size ControlSize = this.animatedControl.Size;
                // Get last bullet in the bullet queue
                CommentBullet lastBullet;
                if (bulletScreen.Bullets.Count > 0)
                    lastBullet = bulletScreen.Bullets.LastOrDefault();
                else
                    lastBullet = null;

                foreach (LiveComment comment in list)
                {
                    // set start position's y value to last bullet's y value
                    if (lastBullet != null && !lastBullet.IsObsolete)
                    {
                        startPosition.Y = lastBullet.Position.Y;
                    }
                    // add comment's height
                    startPosition.Y += comment.Height / (float)ControlSize.Height;
                    // If y value is overflow, set new bullet to top.
                    if (startPosition.Y > 1)
                    {
                        startPosition.Y = comment.Height / (float)ControlSize.Height;
                    }
                    // Add bullet to bullet screen
                    CommentBullet bullet = new CommentBullet(
                                    new Vector2(startPosition.X, startPosition.Y),
                                    comment);
                    bulletScreen.Bullets.Add(bullet);
                    lastBullet = bullet;
                }
            }
        }

        /// <summary>
        /// Set all the bullets in the bullet screen to obsolete
        /// Reload comments to bullet screen        
        /// </summary>
        internal void ReloadComments(uint pagePos)
        {
            bulletScreen.Bullets.ForEach(o => o.IsObsolete = true);
            LoadComments(pagePos);
        }

        /// <summary>
        /// Draw bullets
        /// </summary>
        private void AnimatedControl_Draw(ICanvasAnimatedControl sender,
                                          CanvasAnimatedDrawEventArgs args)
        {
            bulletScreen.Draw(args.DrawingSession, sender.Size);
        }

        /// <summary>
        /// Update bullets
        /// </summary>
        private void AnimatedControl_Update(ICanvasAnimatedControl sender,
                                            CanvasAnimatedUpdateEventArgs args)
        {
            bulletScreen.Update();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate)
            {
                Debug.WriteLine("scroll ongoing");
            }
            else
            {
                Debug.WriteLine("scroll finish");

                var scroll = sender as ScrollViewer;
                var yScroll = scroll.VerticalOffset;
                if (double.IsNaN(yScroll))
                    yScroll = 0;
                var zoom = scroll.ZoomFactor;
                yScroll /= zoom;
                uint currentPagePos = (uint)PdfPages.Count;
                for (int i = 1; i <= PdfPages.Count; ++i)
                {
                    var pageHeight = (double)PdfPages[i - 1].PixelHeight;
                    pageHeight *= scroll.ViewportWidth / PdfPages[i - 1].PixelWidth;
                    yScroll -= pageHeight + scroll.Margin.Top;
                    if (yScroll <= 0f)
                    {
                        currentPagePos = (uint)i;
                        break;
                    }
                }
                ReloadComments(currentPagePos);
            }
        }
    }

    /// <summary>
    /// Bullet Screen
    /// draw and update bullet in bulk
    /// </summary>
    public class BulletScreen
    {
        internal List<CommentBullet> Bullets { get; set; }
        public BulletScreen()
        {
            Bullets = new List<CommentBullet>();
        }

        /// <summary>
        /// Draw bullets
        /// </summary>
        internal void Draw(CanvasDrawingSession ds, Size size)
        {
            Matrix3x2 transform = Matrix3x2.CreateScale(size.ToVector2());
            // Before drawing the bullets, remove the discard bullet first.
            Bullets.RemoveAll(o => o.IsObsolete);
            for (int i = 0; i < Bullets.Count; i++)
            {
                Bullets[i].Draw(ds, transform);
            }
        }

        /// <summary>
        /// Update bullets
        /// </summary>
        internal void Update()
        {
            foreach (CommentBullet bullet in Bullets)
            {
                bullet.Update();
            }
        }
    }

    public class CommentBullet
    {
        // Default speed
        private readonly float MoveSpeed = 0.002f;

        public CommentBullet(Vector2 pos, LiveComment liveComment)
        {
            MoveSpeed += liveComment.Width / 1000000;
            Position = pos;
            CommentItem = liveComment;
            IsObsolete = false;
        }

        public LiveComment CommentItem { get; set; }
        public Vector2 Position { get; set; }
        public bool IsObsolete { get; set; }

        /// <summary>
        /// Draw the bullet
        /// </summary>
        public void Draw(CanvasDrawingSession ds, Matrix3x2 transform)
        {
            var pos = Vector2.Transform(Position, transform);
            // Set center position of bullet
            var center = new Vector2(pos.X + CommentItem.Width / 2, pos.Y - CommentItem.Height / 2);
            //If bullet is out of screen, set to obsolete
            if ((pos.X + CommentItem.Width) < 0)
            {
                IsObsolete = true;
            }
            // Draw avaliable bullet
            if (!IsObsolete)
            {
                ds.DrawText(CommentItem.Comment, center, Util.DanmuColor, Util.DanmuTextFormat);
            }
        }

        /// <summary>
        /// move bullet to left 
        /// </summary>
        public void Update()
        {
            Position = new Vector2(Position.X - MoveSpeed, Position.Y);
        }
    }

    public class LiveComment
    {
        internal Guid ID { get; private set; }

        private readonly Danmu danmu;

        internal string Comment { get => this.danmu.Content; }
        internal uint PageNum { get; private set; }
        internal float Width { get => this.danmu.Content.Length * Util.DanmuSize; }
        internal float Height { get => Util.DanmuSize + Util.DanmuSpacing; }

        public LiveComment(Danmu danmu, uint pagenum)
        {
            this.ID = Guid.NewGuid();
            this.danmu = danmu;
            this.PageNum = pagenum;
        }
    }
}
