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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReadlistPage : Page, IRefreshAdminInterface
    {
        public ReadlistPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            ReadLists = new BooklistCollection(false);
            WaitLoading();
        }

        private async void WaitLoading()
        {
            while (ReadLists.Booklists.Count == 0)
                await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);

            while (true)
            {
                if (ReadLists.Finished)
                {
                    break;
                }
                else
                {
                    await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
                    ReadLists.OnPropertyChanged();
                }
            }
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE * 2);
            ReadLists.OnPropertyChanged();
        }

        internal BooklistCollection ReadLists { set; get; }

        private void Refresh(bool add)
        {
            if (!loadingControl.IsLoading)
            {
                ReadLists.Reload(add);
                WaitLoading();
            }
        }

        private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            this.Refresh(true);
        }

        public void RefreshButtonPressed()
        {
            this.Refresh(false);
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }

        /// <summary>
        /// Show all button, navigate to book list page
        /// </summary>
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var elem = sender as UIElement;
            var parent = elem.GetParentUpto(2);
            if (parent == null || !(parent is ListViewItemPresenter))
                return;
            var collection = (parent as ListViewItemPresenter).DataContext as BookDetailCollection;
            Util.main.NavigateToBooklist(collection.Title, collection.Description, collection.query);
        }

        private BookDetail _navigateItem = null;
        private BookDetailCollection _navigateParentItem = null;

        /// <summary>
        /// Book pressed, Navigate to detail page
        /// </summary>
        private void Book_Pointed(object sender, PointerRoutedEventArgs e)
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

                this._navigateItem = dataToPass;
                this._navigateParentItem = (collectionParent as ListViewItemPresenter).DataContext
                                            as BookDetailCollection;
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
            }
        }

        /// <summary>
        /// Navigate back from detail page
        /// </summary>
        private async void AdaptiveGridView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._navigateParentItem == null || this._navigateItem == null)
                return;
            var animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
            if (animation == null)
                return;
            animation.Configuration = new DirectConnectedAnimationConfiguration();

            if (!(sender is ListViewBase allGrid))
            {
                animation.Cancel();
                return;
            }
            allGrid.ScrollIntoView(this._navigateParentItem);
            if (!(allGrid.ContainerFromItem(this._navigateParentItem) is GridViewItem container))
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
            await boardlist.TryStartConnectedAnimationAsync(animation, this._navigateItem, "bookCover");

            this._navigateParentItem = null;
            this._navigateItem = null;
        }

        internal static string DateFollow(DateTime date, int followers)
        {
            return string.Format("Last edit at:\t{0}\n#Followers:\t{1}", date.ToShortDateString(), followers);
        }
    }
}
