using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BookshelfPage : Page, IRefreshAdminInterface
    {
        public BookshelfPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            Networks.RemoteBookCollection.GetBooksFromQuery(ShelfBooks, Util.SHELF_QUERY);
            WaitLoading();
        }

        internal BookSummaryCollection ShelfBooks { set; get; } = new BookSummaryCollection();

        private async void WaitLoading()
        {
            while (!ShelfBooks.finished)
            {
                await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
            }
            loadingControl.IsLoading = false;
        }

        private void Refresh()
        {
            if (!loadingControl.IsLoading)
            {
                loadingControl.IsLoading = true;
                ShelfBooks.Books.Clear();
                ShelfBooks.finished = false;
                Networks.RemoteBookCollection.GetBooksFromQuery(ShelfBooks, Util.SHELF_QUERY);
                WaitLoading();
            }
        }

        private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            Refresh();
        }

        public void RefreshButtonPressed()
        {
            Refresh();
        }

        public void AdminButtonPressed(bool isChecked)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Navigate to read book page
        /// </summary>
        private void GridView_ItemClick(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}
