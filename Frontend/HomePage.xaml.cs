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
using Windows.UI.Xaml.Navigation;


namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private Dictionary<BookSummaryCollectionType, BookSummaryCollection> collections;

        public ObservableCollection<BookSummary> GetCollections(BookSummaryCollectionType t)
        {
            if (this.collections.ContainsKey(t))
                return this.collections[t].Books;
            else
                return new BookSummaryCollection(t).Books;
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
        }

        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}
