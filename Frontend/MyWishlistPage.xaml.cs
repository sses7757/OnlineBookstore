﻿using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class MyWishlistPage : Page, IRefreshAdminInterface
	{
		public MyWishlistPage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;

			this.Refresh();
		}

		private ObservableCollection<BookDetail> WishBooks { set; get; } = new ObservableCollection<BookDetail>();

		private async void Refresh()
		{
			this.loadingControl.IsLoading = true;
			this.WishBooks.Clear();
			var ids = await NetworkGet.GetMyWishlist();
			foreach (int id in ids)
			{
				var book = new BookDetail(id);
				await NetworkGet.GetBookQuasiDetail(book);
				this.WishBooks.Add(book);
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

		/// <summary>
		/// Navigate to detail page
		/// </summary>
		private void Book_Pointed(object sender, ItemClickEventArgs e)
		{
			var dataToPass = e.ClickedItem as BookDetail;
			if (NetworkGet.IsValidID(dataToPass.ID))
			{
				bookGrid.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "bookCover");
				this._navigateItem = dataToPass;
				Util.MainElem.NavigateToBookDetail(dataToPass, typeof(BookDetailPage));
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
			var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation(Util.FROM_BOOK_DETAIL);
			if (animation != null)
			{
				animation.Configuration = new DirectConnectedAnimationConfiguration();
				bookGrid.ScrollIntoView(this._navigateItem);
				await bookGrid.TryStartConnectedAnimationAsync(animation, this._navigateItem, "bookCover");
			}
		}

		private async void Buy_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
		{
			var bookId = (args.SwipeControl.DataContext as BookDetail).ID;
			string buyURL = await NetworkSet.BuyBook(bookId);
			if (buyURL == null || buyURL.Length <= 4)
				return;
			ContentDialog dialog = new ContentDialog()
			{
				Content = new Image()
				{
					Stretch = Stretch.Uniform,
					Source = await buyURL.ToQRCode()
				},
				Title = "Buying Book",
				IsSecondaryButtonEnabled = true,
				PrimaryButtonText = "I've paid",
				SecondaryButtonText = "Cancel"
			};
			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{ // click finish paying
				var finish = await NetworkSet.CheckBuyComplete(bookId);
				if (finish)
				{
					this.WishBooks.Remove(args.SwipeControl.DataContext as BookDetail);
				}
				else
				{
					notification.Show("Payment failure, please try again later", 4000);
				}
			}
			else
			{
				while(!await NetworkSet.CancleTransaction(bookId)) { }
				notification.Show("Transaction cancled", 4000);
			}
		}

		private async void Delete_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
		{
			var book = args.SwipeControl.DataContext as BookDetail;
			var success = await NetworkSet.ChangeWishlist(book.ID, false);
			if (!success)
				return;
			this.WishBooks.Remove(book);
		}
	}
}
