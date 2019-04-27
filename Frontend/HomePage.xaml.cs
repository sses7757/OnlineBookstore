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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page, IRefreshAdminInterface
    {
        public HomePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.collections = new Dictionary<BookSummaryCollectionType, BookSummaryCollection>
                (Enum.GetValues(typeof(BookSummaryCollectionType)).Length);
            foreach (BookSummaryCollectionType t in Enum.GetValues(typeof(BookSummaryCollectionType)))
            {
                if (t != BookSummaryCollectionType.Other)
                    this.collections.Add(t, new BookSummaryCollection(t));
            }
            this.UpdateLabels();
            WaitLoading();
            Util.LABELS = this.Labels;
        }

        /// <summary>
        /// Navigate back from detail page
        /// </summary>
        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._navigateItem == null)
                return;
            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
            if (animation != null)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                switch (this._navigateType)
                {
                    case BookSummaryCollectionType.NewBooks:
                        NBGrid.ScrollIntoView(this._navigateItem);
                        await NBGrid.TryStartConnectedAnimationAsync(animation, this._navigateItem, "NBImage");
                        break;
                    case BookSummaryCollectionType.TopBooks:
                        TBGrid.ScrollIntoView(this._navigateItem);
                        await TBGrid.TryStartConnectedAnimationAsync(animation, this._navigateItem, "TBImage");
                        break;
                    case BookSummaryCollectionType.PersonalRecommands:
                        PRGrid.ScrollIntoView(this._navigateItem);
                        await PRGrid.TryStartConnectedAnimationAsync(animation, this._navigateItem, "PRImage");
                        break;
                    default:
                        return;
                }
                this._navigateType = BookSummaryCollectionType.Other;
                this._navigateItem = null;
            }
        }

        private BookSummary _navigateItem = null;
        private BookSummaryCollectionType _navigateType = BookSummaryCollectionType.Other;

        /// <summary>
        /// Navigate to detail page
        /// </summary>
        private void GridView_ItemClick(object sender, PointerRoutedEventArgs e)
        {
            var item = sender as StackPanel;
            var dataToPass = item.DataContext as BookSummary;
            if (Networks.IsValidID(dataToPass.BookId))
            {
                switch ((BookSummaryCollectionType)item.Tag)
                {
                    case BookSummaryCollectionType.NewBooks:
                        NBGrid.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "NBImage");
                        break;
                    case BookSummaryCollectionType.TopBooks:
                        TBGrid.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "TBImage");
                        break;
                    case BookSummaryCollectionType.PersonalRecommands:
                        PRGrid.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "PRImage");
                        break;
                    default:
                        return;
                }
                this._navigateType = (BookSummaryCollectionType)item.Tag;
                this._navigateItem = dataToPass;
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));

            }
        }

        private readonly Dictionary<BookSummaryCollectionType, BookSummaryCollection> collections;

        internal ObservableCollection<Label> Labels { set; get; } = new ObservableCollection<Label>();

        internal ObservableCollection<BookSummary> GetCollections(BookSummaryCollectionType t)
        {
            if (this.collections.ContainsKey(t))
                return this.collections[t].Books;
            else
                return new BookSummaryCollection(t).Books;
        }

        private async void WaitLoading()
        {
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE * 2);
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
                    await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
                }
            }
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE * 2);
        }

        private async void UpdateLabels()
        {
            Labels.Clear();
            var mainLabels = await Networks.RemoteGetMainLabels();
            foreach (var s in mainLabels)
            {
                var l = new Label(s);
                Labels.Add(l);
                l.RetriveSubs();
            }
        }

        private void HyperlinkButton_Click_Best(object sender, RoutedEventArgs e)
        {
            Util.main.NavigateToBooklist("Best Selling Books", "System generated recommendations",
                Util.DIRECT_QUERY_PREFIX + 
                BookSummaryCollection.GetStringType(BookSummaryCollectionType.TopBooks));
        }

        private void HyperlinkButton_Click_New(object sender, RoutedEventArgs e)
        {
            Util.main.NavigateToBooklist("Newly Published Books", "System generated recommendations",
                Util.DIRECT_QUERY_PREFIX +
                BookSummaryCollection.GetStringType(BookSummaryCollectionType.NewBooks));
        }

        private void HyperlinkButton_Click_Person(object sender, RoutedEventArgs e)
        {
            Util.main.NavigateToBooklist("Personalized Recommendation", "System generated recommendations",
                Util.DIRECT_QUERY_PREFIX +
                BookSummaryCollection.GetStringType(BookSummaryCollectionType.PersonalRecommands));
        }

        public void RefreshButtonPressed()
        {
            if (!loadingControl.IsLoading)
            {
                foreach (KeyValuePair<BookSummaryCollectionType, BookSummaryCollection> kv in this.collections)
                {
                    kv.Value.Refresh(kv.Key);
                }
                WaitLoading();
                UpdateLabels();
            }
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }

        private void HyperlinkButton_Click_SubLabel(object sender, RoutedEventArgs e)
        {
            // goto search page
        }
    }
}
