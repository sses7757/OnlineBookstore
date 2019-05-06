using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public sealed partial class MyReadlistPage : Page, IRefreshAdminInterface
    {
        public MyReadlistPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.Refresh();
        }

        internal ObservableCollection<BookDetailCollection> ReadLists =
             new ObservableCollection<BookDetailCollection>();

        private async void Refresh()
        {
            this.loadingControl.IsLoading = true;
            this.ReadLists.Clear();
            var ids = await NetworkGet.GetMyReadLists();
            foreach (int id in ids)
            {
                var read = new BookDetailCollection(false, id);
                await read.ReloadBooks(false, id, int.MaxValue); // get all books
                this.ReadLists.Add(read);
            }
            this.loadingControl.IsLoading = false;
        }

        private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            this.Refresh();
        }

        public void RefreshButtonPressed()
        {
            this.Refresh();
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }

        private async void EditTitle_Invoked(object sender, RoutedEventArgs e)
        {
            var swipe = (sender as Button).GetParentUpto(2) as FrameworkElement;
            var books = swipe.DataContext as BookDetailCollection;
            var orgTitle = books.Title;
            var newTitle = await Util.InputTextDialogAsync("Editing Read List Title",
                                                           "Please input the title of your read list",
                                                           orgTitle);
            if (newTitle != null && newTitle.Length > 0 && newTitle != orgTitle)
            {
                if (!books.ID.HasValue)
                {
                    Debug.WriteLine("Read list id not found!");
                }
                var success = await NetworkSet.ChangeReadList(books.ID.Value,
                                                ReadListChangeType.ChangeTitle,
                                                null, newTitle);
                if (!success)
                    return;
                books.Title = newTitle;
                books.EditTime = DateTime.Now;
                books.OnPropertyChanged("Title");
                books.OnPropertyChanged("EditTime");
            }
        }

        private async void EditDesc_Invoked(object sender, RoutedEventArgs e)
        {
            var swipe = (sender as Button).GetParentUpto(2) as FrameworkElement;
            var books = swipe.DataContext as BookDetailCollection;
            var orgDesc = books.Description;
            var newDesc = await Util.InputTextDialogAsync("Editing Read List Description",
                                                           "Please input the description of your read list",
                                                           orgDesc);
            if (newDesc != null && newDesc.Length > 0 && newDesc != orgDesc)
            {
                if (!books.ID.HasValue)
                {
                    Debug.WriteLine("Read list id not found!");
                }
                var success = await NetworkSet.ChangeReadList(books.ID.Value,
                                                ReadListChangeType.ChangeDescription,
                                                null, newDesc);
                if (!success)
                    return;
                books.Description = newDesc;
                books.EditTime = DateTime.Now;
                books.OnPropertyChanged("Description");
                books.OnPropertyChanged("EditTime");
            }
        }

        private async void DeleteList_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            var books = args.SwipeControl.DataContext as BookDetailCollection;
            ContentDialog dialog = new ContentDialog()
            {
                Content = "Are you sure to delete the whole read lists?" +
                          "\r\nThis operation is irrevesable.",
                Title = "Deleting Read List",
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Confirm",
                SecondaryButtonText = "Cancel"
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (!books.ID.HasValue)
                {
                    Debug.WriteLine("Read list id not found!");
                }
                var success = await NetworkSet.ChangeReadList(books.ID.Value,
                                                ReadListChangeType.RemoveReadList);
                if (!success)
                    return;
                this.ReadLists.Remove(args.SwipeControl.DataContext as BookDetailCollection);
            }
        }

        private async void DeleteBook_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            var parent = args.SwipeControl.GetParentUpto(Util.LEVEL_DataTemplate + 6)
                            as ListViewItemPresenter;
            var books = parent.DataContext as BookDetailCollection;
            if (!books.ID.HasValue)
            {
                Debug.WriteLine("Read list id not found!");
            }
            var success = await NetworkSet.ChangeReadList(books.ID.Value,
                                            ReadListChangeType.DeleteBook,
                                            (args.SwipeControl.DataContext as BookDetail).ID);
            if (!success)
                return;
            books.Books.Remove(args.SwipeControl.DataContext as BookDetail);
            books.EditTime = DateTime.Now;
            books.OnPropertyChanged("EditTime");
        }

        private BookDetail _navigateItem = null;
        private BookDetailCollection _navigateParentItem = null;

        /// <summary>
        /// Book pressed, Navigate to detail page
        /// </summary>
        private void Book_Pointed(object sender, ItemClickEventArgs e)
        {
            var parent = sender as ListViewBase;
            var dataToPass = e.ClickedItem as BookDetail;
            if (NetworkGet.IsValidID(dataToPass.ID))
            {
                var collectionParent = parent.GetParentUpto(6) as ListViewItemPresenter;
                parent.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "bookCover");
                var service = ConnectedAnimationService.GetForCurrentView();
                service.DefaultDuration = TimeSpan.FromSeconds(0.45);

                this._navigateItem = dataToPass;
                this._navigateParentItem = collectionParent.DataContext as BookDetailCollection;
                Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
            }
        }

        /// <summary>
        /// Navigate back from detail page
        /// </summary>
        private async void ListView_Loaded(object sender, RoutedEventArgs e)
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
            if (!(allGrid.ContainerFromItem(this._navigateParentItem) is ListViewItem container))
            {
                animation.Cancel();
                return;
            }
            if (!(((container.ContentTemplateRoot as SwipeControl).Content as Grid).Children
                    is UIElementCollection child))
            {
                animation.Cancel();
                return;
            }
            if (!(child[child.Count - 1] is ListViewBase boardlist))
            {
                animation.Cancel();
                return;
            }
            await boardlist.TryStartConnectedAnimationAsync(animation, this._navigateItem, "bookCover");

            this._navigateParentItem = null;
            this._navigateItem = null;
        }
    }
}
