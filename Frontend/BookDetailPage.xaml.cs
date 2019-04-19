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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BookDetailPage : Page, INotifyPropertyChanged, RefreshAdminInterface
    {
        public BookDetailPage()
        {
            this.InitializeComponent();
        }

        private BookDetail detail;
        public BookDetail Detail {
            get { return detail; }
            set { detail = value; OnPropertyChanged("Detail"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static string PublishInfoAndPage(BookDetail detail)
        {
            return string.Format("{0} / {1} pages", detail.PublishInfo, detail.PageCount);
        }

        public static string RatingReviewCount(BookDetail detail)
        {
            return string.Format("({0:N1}, {1} reviews)", detail.OverallRating, detail.ReviewAmount);
        }

        public static string PriceDiscount(BookDetail detail)
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

        public static string GetAllAuthors(BookDetail detail)
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

        public static string OtherStatistic(BookDetail detail)
        {
            return string.Format("{0} bullet-screen comments & {1} previews",
                                 detail.DanmuAmount, detail.PreviewAmount);
        }

        private async void Refresh()
        {
            await System.Threading.Tasks.Task.Delay(500);
            while (!detail.finished)
            {
                await System.Threading.Tasks.Task.Delay(100);
                Detail = detail;
            }
            await System.Threading.Tasks.Task.Delay(500);
            Detail = detail;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var bookSummary = (BookSummary)e.Parameter;
            detail = new BookDetail(bookSummary);
            Refresh();
        }

        private void StackPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var dataToPass = (BookSummary)((StackPanel)sender).DataContext;
            if (dataToPass.BookId > 0)
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage2));
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
            detail.GetMoreReview();
            Refresh();
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // add edit info button
        }
    }
}
