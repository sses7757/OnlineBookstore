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


namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BooklistPage : Page, INotifyPropertyChanged, IRefreshAdminInterface
    {
        public BooklistPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var para = (string[])e.Parameter;
            if (this.title == para[0] && this.description == para[1] && this.queryText == para[2])
                return;
            this.title = para[0];
            this.description = para[1];
            this.queryText = para[2];
            this.books = new BookDetailCollection(this.queryText, this.title, this.description);
            _ = this.RefreshAsync();
        }

        /// <summary>
        /// Navigate to detail page
        /// </summary>
        private void BookCover_Pointed(object sender, PointerRoutedEventArgs e)
        {
            var dataToPass = (sender as FrameworkElement).DataContext as BookDetail;
            if (Networks.IsValidID(dataToPass.BookId))
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
            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
            if (animation != null)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                bookGrid.ScrollIntoView(this._navigateItem);
                await bookGrid.TryStartConnectedAnimationAsync(animation, this._navigateItem, "bookCover");
            }
        }
        private string queryText = "";
        private string title = "";
        private string description = "";

        private BookDetailCollection books;

        internal BookDetailCollection Books {
            get { return books; }
            set { books = value; OnPropertyChanged("Books"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async System.Threading.Tasks.Task<bool> RefreshAsync()
        {
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
            while (!books.finished)
            {
                await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
                Books = books;
            }
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE * 2);
            Books = books;
            return true;
        }

        internal static string PriceDiscount(double price, int discount)
        {
            if (discount == 100)
            {
                return string.Format("Price:\t{0:C2}", price);
            }
            else
            {
                return string.Format("Price:\t{0:C2} ({1}% OFF)", price, 100 - discount);
            }
        }

        private async void RefreshPage(bool addBooks)
        {
            if (addBooks)
            {
                this.books.AddBooks();
                await this.RefreshAsync();
                bookGrid.ScrollIntoView(this.books.Books[this.books.Books.Count - 1]);
            }
            else
            {
                this.books = new BookDetailCollection(this.queryText, this.title, this.description);
                _ = this.RefreshAsync();
            }
        }

        private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            this.RefreshPage(true);
        }

        public void RefreshButtonPressed()
        {
            this.RefreshPage(false);
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }
    }
}
