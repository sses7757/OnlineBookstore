using Microsoft.Toolkit.Uwp.UI.Controls;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page, IRefreshAdminInterface
    {
        public SearchPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        internal SearchInfo Info { set; get; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var para = e.Parameter as SearchInfo;
            if (Info == null || Info.QueryText == null)
            {
                Info = para;
                return;
            }
            if (para.QueryText != Info.QueryText)
            {
                Info = para;
                Tabs.SelectedIndex = 0;
                this.Tabs_SelectionChanged(Tabs, null);
            }
        }

        private void QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Info.QueryText = sender.Text;
            Info.OnPropertyChanged("QueryText");
        }

        private void PageRange_Changed(object sender, RangeChangedEventArgs e)
        {
            var rangeSelector = sender as RangeSelector;
            Info.PageRange.Minimum = Convert.ToInt32(rangeSelector.RangeMin);
            Info.PageRange.Maximum = Convert.ToInt32(rangeSelector.RangeMax);
            Info.OnPropertyChanged("PageRange");
        }

        private void TimeRangeSelector_ValueChanged(object sender, RangeChangedEventArgs e)
        {
            var rangeSelector = sender as RangeSelector;
            Info.TimeRange.Minimum = Convert.ToInt32(rangeSelector.RangeMin);
            Info.TimeRange.Maximum = Convert.ToInt32(rangeSelector.RangeMax);
            Info.OnPropertyChanged("TimeRange");
        }

        private async void TimeRangeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timeRangeSelector == null ||
                (sender as ComboBox).SelectedIndex + 1 > Enum.GetNames(typeof(TimeSpanType)).Length)
            {
                await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE * 2);
                TimeRangeType_SelectionChanged(sender, e);
                return;
            }
            switch ((TimeSpanType)(sender as ComboBox).SelectedIndex)
            {
                case TimeSpanType.All:
                    timeRangeSelector.IsEnabled = false;
                    timeRangeSelector.Minimum = SearchInfo.MIN_YEAR_RANGE;
                    timeRangeSelector.RangeMin = timeRangeSelector.Minimum;
                    break;
                case TimeSpanType.Year:
                    timeRangeSelector.IsEnabled = true;
                    timeRangeSelector.Minimum = SearchInfo.MIN_YEAR_RANGE;
                    timeRangeSelector.RangeMin = Math.Max(timeRangeSelector.Minimum, timeRangeSelector.RangeMin);
                    break;
                case TimeSpanType.Month:
                    timeRangeSelector.IsEnabled = true;
                    timeRangeSelector.Minimum = SearchInfo.MIN_MONTH_RANGE;
                    timeRangeSelector.RangeMin = Math.Max(timeRangeSelector.Minimum, timeRangeSelector.RangeMin);
                    break;
                case TimeSpanType.Week:
                    timeRangeSelector.IsEnabled = true;
                    timeRangeSelector.Minimum = SearchInfo.MIN_WEEK_RANGE;
                    timeRangeSelector.RangeMin = Math.Max(timeRangeSelector.Minimum, timeRangeSelector.RangeMin);
                    break;
                default:
                    return;
            }
            TimeRangeSelector_ValueChanged(timeRangeSelector, null);
        }

        private async void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null ||
                (sender as TabView).SelectedIndex + 1 > Enum.GetNames(typeof(ContentType)).Length)
                return;
            Info.QueryType = (ContentType)(sender as TabView).SelectedIndex;
            Info.OnPropertyChanged("OrderItems");
            if (Info.QueryType != ContentType.Books &&
                (Info.QueryType == ContentType.Billboards ? Info.Billboards : Info.ReadLists).Booklists.Count == 0)
                Info.Refresh(false, true);
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE / 4);
            orderCombo.SelectedIndex = 0;
        }

        private void OrderBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox combo) ||
                combo.SelectedIndex + 1 > Info.OrderItems.Count ||
                combo.SelectedIndex == Info.OrderToIndex())
            {
                return;
            }
            Info.FromIndexSetOrder(combo.SelectedIndex);
        }

        private async void LabelToggle_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton toggle))
            {
                return;
            }
            
            var label = toggle.DataContext;
            if (label == null)
            {
                return;
            }

            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE / 4);
            if (label is Label)
            {
                (label as Label).OnPropertyChanged("Selected");
            }
            else if (label is SubLabel)
            {
                (label as SubLabel).OnPropertyChanged("Selected");
                (label as SubLabel).Parent.OnPropertyChanged("SelectedSubs");
                (label as SubLabel).Parent.CheckSubLabelFull();
            }
            Info.LabelFilterChanged();
        }

        private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            Info.Refresh(true);
        }

        public void RefreshButtonPressed()
        {
            Info.Refresh(false);
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Info.OnPropertyChanged(null);
        }

        private (ContentType? type, BookDetail item, BookDetailCollection parent) _nav;

        private void ClearNav()
        {
            this._nav.parent = null;
            this._nav.item = null;
            this._nav.type = null;
        }

        /// <summary>
        /// Navigate to detail page
        /// </summary>
        private void Direct_Book_Pointed(object sender, PointerRoutedEventArgs e)
        {
            var dataToPass = (sender as FrameworkElement).DataContext as BookDetail;
            if (NetworkGet.IsValidID(dataToPass.BookId))
            {
                bookGrid.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "bookCover");
                this._nav.item = dataToPass;
                this._nav.type = ContentType.Books;
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
            }
        }

        /// <summary>
        /// Navigate back from detail page
        /// </summary>
        private async void Direct_BookGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._nav.type.HasValue || this._nav.type.Value != ContentType.Books ||
                this._nav.item == null)
                return;
            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
            if (animation != null)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                bookGrid.ScrollIntoView(this._nav.item);
                await bookGrid.TryStartConnectedAnimationAsync(animation, this._nav.item, "bookCover");
            }
            this.ClearNav();
        }

        /// <summary>
        /// Show all button of billboards, navigate to book list page
        /// </summary>
        private void Billboard_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var elem = sender as UIElement;
            var parent = elem.GetParentUpto(2);
            if (parent == null || !(parent is ListViewItemPresenter))
                return;
            var collection = (parent as ListViewItemPresenter).DataContext as BookDetailCollection;
            Util.main.NavigateToBooklist(collection.Title, collection.Description, collection.query);
        }

        /// <summary>
        /// Book in billboard pressed, Navigate to detail page
        /// </summary>
        private void Billboard_Book_Pointed(object sender, PointerRoutedEventArgs e)
        {
            var elem = sender as Grid;
            var dataToPass = elem.DataContext as BookDetail;
            if (NetworkGet.IsValidID(dataToPass.BookId))
            {
                var parent = elem.GetParentUpto(Util.LEVEL_DataTemplate);
                var collectionParent = parent.GetParentUpto(2);
                if (parent == null || !(parent is ListView) ||
                    collectionParent == null || !(collectionParent is ListViewItemPresenter))
                    return;

                (parent as ListView).PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "bookCover");
                var service = ConnectedAnimationService.GetForCurrentView();
                service.DefaultDuration = TimeSpan.FromSeconds(0.45);

                this._nav.item = dataToPass;
                this._nav.parent = (collectionParent as ListViewItemPresenter).DataContext
                                            as BookDetailCollection;
                this._nav.type = ContentType.Billboards;
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
            }
        }

        /// <summary>
        /// Navigate back from detail page
        /// </summary>
        private async void Billboard_Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._nav.type.HasValue || this._nav.type.Value != ContentType.Billboards ||
                this._nav.parent == null || this._nav.item == null)
                return;
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
            if (animation == null)
                return;
            animation.Configuration = new DirectConnectedAnimationConfiguration();

            if (!(sender is ListViewBase allGrid))
            {
                animation.Cancel();
                return;
            }
            allGrid.ScrollIntoView(this._nav.parent);
            if (!(allGrid.ContainerFromItem(this._nav.parent) is GridViewItem container))
            {
                animation.Cancel();
                return;
            }
            if (!((container.ContentTemplateRoot as Grid).Children
                           [(container.ContentTemplateRoot as Grid).Children.Count - 1] is ListView boardlist))
            {
                animation.Cancel();
                return;
            }
            await boardlist.TryStartConnectedAnimationAsync(animation, this._nav.item, "bookCover");

            this.ClearNav();
        }

        /// <summary>
        /// Show all button of read list, navigate to book list page
        /// </summary>
        private void ReadList_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var elem = sender as UIElement;
            var parent = elem.GetParentUpto(2);
            if (parent == null || !(parent is ListViewItemPresenter))
                return;
            var collection = (parent as ListViewItemPresenter).DataContext as BookDetailCollection;
            Util.main.NavigateToBooklist(collection.Title, collection.Description, collection.query);
        }

        /// <summary>
        /// Book in read list pressed, Navigate to detail page
        /// </summary>
        private void ReadList_Book_Pointed(object sender, PointerRoutedEventArgs e)
        {
            var elem = sender as Grid;
            var dataToPass = elem.DataContext as BookDetail;
            if (NetworkGet.IsValidID(dataToPass.BookId))
            {
                var parent = elem.GetParentUpto(Util.LEVEL_DataTemplate);
                var collectionParent = parent.GetParentUpto(2);
                if (parent == null || !(parent is ListView) ||
                    collectionParent == null || !(collectionParent is ListViewItemPresenter))
                    return;

                (parent as ListView).PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "bookCover");
                var service = ConnectedAnimationService.GetForCurrentView();
                service.DefaultDuration = TimeSpan.FromSeconds(0.45);

                this._nav.item = dataToPass;
                this._nav.parent = (collectionParent as ListViewItemPresenter).DataContext
                                    as BookDetailCollection;
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
            }
        }

        /// <summary>
        /// Navigate back from detail page
        /// </summary>
        private async void ReadList_Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._nav.type.HasValue || this._nav.type.Value != ContentType.ReadLists ||
                this._nav.parent == null || this._nav.item == null)
                return;
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
            if (animation == null)
                return;
            animation.Configuration = new DirectConnectedAnimationConfiguration();

            if (!(sender is ListViewBase allGrid))
            {
                animation.Cancel();
                return;
            }
            allGrid.ScrollIntoView(this._nav.parent);
            if (!(allGrid.ContainerFromItem(this._nav.parent) is GridViewItem container))
            {
                animation.Cancel();
                return;
            }
            if (!((container.ContentTemplateRoot as Grid).Children
                           [(container.ContentTemplateRoot as Grid).Children.Count - 1] is ListView boardlist))
            {
                animation.Cancel();
                return;
            }
            await boardlist.TryStartConnectedAnimationAsync(animation, this._nav.item, "bookCover");

            this.ClearNav();
        }
    }
}
