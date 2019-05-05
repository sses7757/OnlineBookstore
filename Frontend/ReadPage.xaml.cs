using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var paras = ((string key, string url))e.Parameter;
            this.privateKey = paras.key;
            this.Source = new Uri(paras.url);
            this.LoadAsync();
        }

        private string privateKey;
        internal Uri Source { private set; get; }
        internal ObservableCollection<BitmapImage> PdfPages {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();


        private async void LoadAsync()
        {
            if (Source == null)
            {
                PdfPages.Clear();
            }
            else
            {
                if (Source.IsFile || !Source.IsWebUri())
                {
                    await LoadFromLocalAsync();
                }
                else if (Source.IsWebUri())
                {
                    await LoadFromRemoteAsync();
                }
                else
                {
                    Debug.WriteLine($"Source '{Source.ToString()}' could not be recognized!");
                }
            }
        }

        private async Task LoadFromRemoteAsync()
        {
            HttpClient client = new HttpClient();
            var stream = await client.GetStreamAsync(Source);
            var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            memStream.Position = 0;
            var doc = await PdfDocument.LoadFromStreamAsync(memStream.AsRandomAccessStream());

            Load(doc);
        }

        private async Task LoadFromLocalAsync()
        {
            var f = await StorageFile.GetFileFromApplicationUriAsync(Source);
            PdfDocument doc = await PdfDocument.LoadFromFileAsync(f);

            Load(doc);
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
    }
}
