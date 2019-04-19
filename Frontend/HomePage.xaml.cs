using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page, RefreshAdminInterface
    {
        private Dictionary<BookSummaryCollectionType, BookSummaryCollection> collections;

        public ObservableCollection<BookSummary> GetCollections(BookSummaryCollectionType t)
        {
            if (this.collections.ContainsKey(t))
                return this.collections[t].Books;
            else
                return new BookSummaryCollection(t).Books;
        }

        private async void WaitLoading()
        {
            while (true)
            {
                var load = false;
                foreach (KeyValuePair<BookSummaryCollectionType, BookSummaryCollection> kv in collections)
                {
                    if (!kv.Value.finished)
                    {
                        load = true;
                        break;
                    }
                }
                if (!load)
                {
                    loadingControl.IsLoading = false;
                    break;
                }
                else
                {
                    await System.Threading.Tasks.Task.Delay(500);
                }
            }
        }

        public HomePage()
        {
            this.collections = new Dictionary<BookSummaryCollectionType, BookSummaryCollection>
                (Enum.GetValues(typeof(BookSummaryCollectionType)).Length);
            foreach (BookSummaryCollectionType t in Enum.GetValues(typeof(BookSummaryCollectionType)))
            {
                this.collections.Add(t, new BookSummaryCollection(t));
            }
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            WaitLoading();
        }

        private void GridView_ItemClick(object sender, PointerRoutedEventArgs e)
        {
            var dataToPass = (BookSummary)((StackPanel)sender).DataContext;
            if (dataToPass.BookId > 0)
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
        }

        private void HyperlinkButton_Click_Best(object sender, RoutedEventArgs e)
        {
            
        }

        private void HyperlinkButton_Click_New(object sender, RoutedEventArgs e)
        {
            
        }

        public void RefreshButtonPressed()
        {
            if (!loadingControl.IsLoading)
            {
                this.collections = new Dictionary<BookSummaryCollectionType, BookSummaryCollection>
                (Enum.GetValues(typeof(BookSummaryCollectionType)).Length);
                foreach (BookSummaryCollectionType t in Enum.GetValues(typeof(BookSummaryCollectionType)))
                {
                    this.collections.Add(t, new BookSummaryCollection(t));
                }
                WaitLoading();
            }
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }
    }
}
