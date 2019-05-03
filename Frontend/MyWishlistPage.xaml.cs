using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class MyWishlistPage : Page, IRefreshAdminInterface
    {
        public MyWishlistPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.Refresh();
        }

        internal ObservableCollection<BookDetail> WishBooks = new ObservableCollection<BookDetail>();

        private async void Refresh()
        {
            this.loadingControl.IsLoading = true;
            this.WishBooks.Clear();
            var ids = await NetworkGet.GetMyWishlist();
            foreach (int id in ids)
            {
                var book = new BookDetail(id);
                await NetworkGet.GetBookQuasiDetail(book);
                this.WishBooks.Add(book);
            }
            this.loadingControl.IsLoading = false;
        }

        private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            this.Refresh();
        }

        public void RefreshButtonPressed()
        {
            this.Refresh();
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }

        /// <summary>
        /// Navigate to detail page
        /// </summary>
        private void Book_Pointed(object sender, PointerRoutedEventArgs e)
        {
            var dataToPass = (sender as FrameworkElement).DataContext as BookDetail;
            if (NetworkGet.IsValidID(dataToPass.BookId))
            {
                bookGrid.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "bookCover");
                this._navigateItem = dataToPass;
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
            }
        }

        private BookDetail _navigateItem = null;

        /// <summary>
        /// Navigate back from detail page
        /// </summary>
        private async void BookGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._navigateItem == null)
                return;
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
            if (animation != null)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                bookGrid.ScrollIntoView(this._navigateItem);
                await bookGrid.TryStartConnectedAnimationAsync(animation, this._navigateItem, "bookCover");
            }
        }

        private void Buy_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            // TODO network buy
        }

        private void Delete_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            // TODO network delete
            this.WishBooks.Remove(args.SwipeControl.DataContext as BookDetail);
        }
    }
}
