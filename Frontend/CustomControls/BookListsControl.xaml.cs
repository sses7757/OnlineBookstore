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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Frontend.CustomControls
{
	public sealed partial class BookListsControl : UserControl
	{
		public BookListsControl()
		{
			this.InitializeComponent();
		}

		internal BooklistCollection Booklist { set; get; }

		public int PaddingX { set; get; }

		public Action RefreshRequest { set; get; } = null;

		public bool IsBillboard { set; get; }

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
			if (!((container.ContentTemplateRoot as Grid).Children
						   [(container.ContentTemplateRoot as Grid).Children.Count - 1] is ListView boardlist))
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
			if (this.RefreshRequest == null)
				this.Refresh(true);
			else
				this.RefreshRequest();
		}
	}
}
