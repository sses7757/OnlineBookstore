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
            Info = para;
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
            // TODO
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Info.OnPropertyChanged(null);
        }
    }
}
