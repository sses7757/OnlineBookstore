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
    public sealed partial class BookDetailPage2 : Page, INotifyPropertyChanged, RefreshAdminInterface
    {
        public BookDetailPage2()
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
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

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
