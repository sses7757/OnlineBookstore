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
    public sealed partial class BooklistPage : Page, INotifyPropertyChanged, RefreshAdminInterface
    {
        public BooklistPage()
        {
            this.InitializeComponent();
        }

        private string queryText = "";
        public string Title { set; get; }
        public string Description { set; get; }

        private BookDetailCollection books;

        public BookDetailCollection Books {
            get { return books; }
            set { books = value; OnPropertyChanged("Books"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void Refresh()
        {
            await System.Threading.Tasks.Task.Delay(500);
            while (!books.finished)
            {
                await System.Threading.Tasks.Task.Delay(100);
                Books = books;
            }
            await System.Threading.Tasks.Task.Delay(500);
            Books = books;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var para = (string[])e.Parameter;
            Title = para[0];
            Description = para[1];
            this.queryText = para[2];
            this.books = new BookDetailCollection(this.queryText);
            Refresh();
        }

        public static string PriceDiscount(double price, int discount)
        {
            if (discount == 100)
            {
                return string.Format("{0:C2}", price);
            }
            else
            {
                return string.Format("{0:C2} ({1}% OFF)", price, 100 - discount);
            }
        }

        private void BookCover_Pointed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void RefreshPage(bool addBooks)
        {
            // TODO
            Refresh();
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
