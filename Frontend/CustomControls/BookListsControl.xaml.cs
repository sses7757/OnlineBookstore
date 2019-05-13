﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Frontend.CustomControls
{
	public sealed partial class BookListsControl : UserControl
	{
		public enum IconType
		{
			AddIcon,
			RemoveIcon,
			BuyIcon,
			EditIcon,
			DeleteIcon
		}

		public BookListsControl()
		{
			this.InitializeComponent();
		}

		public BooklistCollection Booklist { set; get; }

		public bool ShowTopSwipe { set; get; } = false;
		public bool ShowLeftSwipe { set; get; } = false;
		public bool IsTopSwipeFollow { set; get; } = true;

		internal void UpdateSwipes()
		{
			foreach (var collection in this.Booklist?.Booklists)
			{
				if (collection != null)
					collection.ShowFollowSwipe = this.IsTopSwipeFollow;
			}
		}

		public string LeftSwipeText { set; get; } = "Remove book";
		public IconType LeftSwipeIcon { set; get; } = IconType.DeleteIcon;

		private IconSource LeftIconSource {
			get => Application.Current.Resources[LeftSwipeIcon.ToString()] as IconSource;
		}

		public bool CanEdit { set; get; } = false;

		public int PaddingX { set; get; }
		public Action RefreshOverride { set; get; } = null;
		public bool IsBillboard { set; get; }


		private Visibility TextBoxVisibility { get => CanEdit.ToVisibility(); }
		private Visibility TextBlockVisibility { get => (!CanEdit).ToVisibility(); }

		private Thickness OutPadding { get => new Thickness(this.PaddingX, 0, this.PaddingX, 0); }
		private Visibility UserInfoVisibility { get => (!this.IsBillboard).ToVisibility(); }

		/// <summary>
		/// Show all button of read list, navigate to book list page
		/// </summary>
		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			var elem = sender as UIElement;
			var parent = elem.GetParentUpto(2);
			if (parent == null || !(parent is ListViewItemPresenter))
				return;
			var collection = (parent as ListViewItemPresenter).DataContext as BookDetailCollection;
			Util.main.NavigateToBooklist(collection.Title, collection.Description, collection.query);
		}

		private (BookDetail item, BookDetailCollection parent) _nav;

		/// <summary>
		/// Navigate back from detail page
		/// </summary>
		private async void AllGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if (this._nav.parent == null || this._nav.item == null)
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
			await boardlist.TryStartConnectedAnimationAsync(animation, this._nav.item, "bookCover");

			this._nav.item = null;
			this._nav.parent = null;
		}

		/// <summary>
		/// Book in read list pressed, Navigate to detail page
		/// </summary>
		private void Book_ItemClick(object sender, ItemClickEventArgs e)
		{
			var listView = sender as ListViewBase;
			var dataToPass = e.ClickedItem as BookDetail;
			if (NetworkGet.IsValidID(dataToPass.ID))
			{
				listView.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "bookCover");
				var service = ConnectedAnimationService.GetForCurrentView();
				service.DefaultDuration = TimeSpan.FromSeconds(0.45);

				this._nav.item = dataToPass;
				this._nav.parent = listView.DataContext as BookDetailCollection;
				Util.main.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
			}
		}

		internal async void WaitLoading()
		{
			UpdateSwipes();
			loadingControl.IsLoading = true;
			while (Booklist.Booklists.Count == 0)
				await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);

			while (true)
			{
				if (Booklist.Finished)
				{
					break;
				}
				else
				{
					await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
					Booklist.OnPropertyChanged();
				}
			}
			await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE * 2);
			Booklist.OnPropertyChanged();
			loadingControl.IsLoading = false;
		}

		internal void Refresh(bool add = false)
		{
			if (!loadingControl.IsLoading)
			{
				Booklist.Reload(add);
				WaitLoading();
			}
		}

		private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			if (this.RefreshOverride == null)
				this.Refresh(true);
			else
				this.RefreshOverride();
		}

		private async void Top_SwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
		{
			if (!(args.SwipeControl.DataContext is BookDetailCollection collection) ||
				!collection.ID.HasValue)
			{
				System.Diagnostics.Debug.WriteLine("Top swipe item error!");
				return;
			}
			var id = collection.ID.Value;
			if (this.IsTopSwipeFollow)
			{
				bool success = await NetworkSet.FollowReadList(id, !collection.Followed);
				if (success)
				{
					collection.Followed = !collection.Followed;
					collection.OnPropertyChanged("SwipeString");
					collection.OnPropertyChanged("SwipeIcon");
				}
			}
			else
			{
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
					bool success = await NetworkSet.ChangeReadList(id, BookListChangeType.RemoveList);
					if (success)
						this.Booklist.Booklists.Remove(collection);
				}
			}
		}

		private async void Left_SwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
		{
			if (!(args.SwipeControl.DataContext is BookDetail book))
			{
				System.Diagnostics.Debug.WriteLine("Top swipe item error!");
				return;
			}
			var parent = args.SwipeControl.GetParentUpto(Util.LEVEL_DataTemplate + 5) as SwipeControl;
			var collection = parent?.DataContext as BookDetailCollection;
			if (collection == null || !collection.ID.HasValue)
			{
				System.Diagnostics.Debug.WriteLine("Top swipe item error!");
			}
			var id = collection.ID.Value;
			var success = await NetworkSet.ChangeReadList(id, BookListChangeType.DeleteBook, book.ID);
			if (success)
			{
				collection.Books.Remove(book);
				collection.EditTime = DateTime.Now;
				collection.OnPropertyChanged("EditTime");
			}
		}

		private async void TextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
		{
			if (e.Key == Windows.System.VirtualKey.Enter)
			{
				var box = sender as TextBox;
				var newText = box?.Text;
				var collection = (box.GetParentUpto(2) as FrameworkElement).DataContext as BookDetailCollection;
				var oldText = collection?.Title;
				if (newText == null || newText.Length <= 2 || newText == oldText)
					return;
				bool success;
				if ((sender as TextBox).Tag as string == "title")
				{
					success = await EditTitle_Invoked(newText, collection);
				}
				else
				{
					success = await EditDesc_Invoked(newText, collection);
				}
				if (!success)
					box.Text = oldText;
			}
		}

		private async Task<bool> EditTitle_Invoked(string newTitle, BookDetailCollection collection)
		{
			var success = await NetworkSet.ChangeReadList(collection.ID.Value,
															BookListChangeType.ChangeTitle,
															null, newTitle);
			if (!success)
				return false;
			collection.Title = newTitle;
			collection.EditTime = DateTime.Now;
			collection.OnPropertyChanged("Title");
			collection.OnPropertyChanged("EditTime");
			return true;
		}

		private async Task<bool> EditDesc_Invoked(string newDesc, BookDetailCollection collection)
		{
			var success = await NetworkSet.ChangeReadList(collection.ID.Value,
															BookListChangeType.ChangeDescription,
															null, newDesc);
			if (!success)
				return false;
			collection.Description = newDesc;
			collection.EditTime = DateTime.Now;
			collection.OnPropertyChanged("Description");
			collection.OnPropertyChanged("EditTime");
			return true;
		}
	}
}