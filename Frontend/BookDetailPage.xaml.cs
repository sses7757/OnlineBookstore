using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BookDetailPage : Page, INotifyPropertyChanged, IRefreshAdminInterface
    {
        public BookDetailPage()
        {
            this.InitializeComponent();
            //this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        /// <summary>
        /// Navigate from list pages, etc.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // load details first
            var bookSummary = (BookSummary)e.Parameter;
            detail = new BookDetail(bookSummary);
            Refresh();
            // start animations
            var animationService = ConnectedAnimationService.GetForCurrentView();
            var animation = animationService.GetAnimation(Util.TO_BOOK_DETAIL);
            if (animation != null)
            {
                animation.TryStart(bookCover, new UIElement[] { anchorGrid });
            }
        }

        /// <summary>
        /// Navigate back to list pages, etc.
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                var service = ConnectedAnimationService.GetForCurrentView();
                service.PrepareToAnimate(Util.FROM_BOOK_DETAIL, bookCover);
                // Use the recommended configuration for back animation.
                service.GetAnimation(Util.FROM_BOOK_DETAIL).Configuration =
                    new DirectConnectedAnimationConfiguration();
            }
        }

        /// <summary>
        /// Navigate to another detail page
        /// </summary>
        private void StackPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var dataToPass = (BookSummary)((StackPanel)sender).DataContext;
            if (dataToPass.BookId > 0)
            {
                relatedBookGrid.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "relateBookImage");
                this._navigateItem = dataToPass;
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
            }
        }

        private BookSummary _navigateItem = null;

        /// <summary>
        /// Navigate back from another detail page
        /// </summary>
        private async void RelatedBookGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._navigateItem == null)
                return;
            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
            if (animation != null)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                relatedBookGrid.ScrollIntoView(this._navigateItem);
                await relatedBookGrid.TryStartConnectedAnimationAsync(animation,
                                                                      this._navigateItem, "relateBookImage");
            }
        }

        private BookDetail detail;
        internal BookDetail Detail {
            get { return detail; }
            set { detail = value; OnPropertyChanged("Detail"); }
        }

        internal System.Collections.ObjectModel.ObservableCollection<BookSummary> RelatedBooks {
            get { return Detail.RelatedBooks.Books; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal static string PublishInfoAndPage(BookDetail detail)
        {
            return string.Format("{0} / {1} pages", detail.PublishInfo, detail.PageCount);
        }

        internal static string RatingReviewCount(BookDetail detail)
        {
            return string.Format("({0:N1}, {1} reviews)", detail.OverallRating, detail.ReviewAmount);
        }

        internal static string PriceDiscount(BookDetail detail)
        {
            if (detail.Discount == 100)
            {
                return string.Format("{0:C2} ({1} buyers till now)", detail.Price, detail.BuyAmount);
            }
            else
            {
                return string.Format("{0:C2} ({1}% OFF) ({2} buyers till now)",
                                     detail.Price, 100 - detail.Discount, detail.BuyAmount);
            }
        }

        internal static string GetAllAuthors(BookDetail detail)
        {
            var str = "";
            if (detail.OtherAuthors == null || detail.OtherAuthors.Trim().Length == 0)
            {
                str += detail.Author;
            }
            else
            {
                str += detail.Author + "; " + detail.OtherAuthors;
            }
            return str;
        }

        internal static string OtherStatistic(BookDetail detail)
        {
            return string.Format("{0} bullet-screen comments & {1} previews",
                                 detail.DanmuAmount, detail.PreviewAmount);
        }

        private async void Refresh()
        {
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
            while (!detail.finished)
            {
                await System.Threading.Tasks.Task.Delay(100);
                Detail = detail;
                OnPropertyChanged("RelatedBooks");
            }
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
            Detail = detail;
            OnPropertyChanged("RelatedBooks");
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO : goto booklist page
        }

        private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
            {
                this.RefreshButtonPressed();
            }
        }

        public void RefreshButtonPressed()
        {
            detail = new BookDetail(detail as BookSummary);
            Refresh();
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // add edit info button
        }
    }
}
