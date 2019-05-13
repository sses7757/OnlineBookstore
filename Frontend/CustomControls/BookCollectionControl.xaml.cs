using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Frontend.CustomControls
{
	public sealed partial class BookCollectionControl : UserControl
	{
		public BookCollectionControl()
		{
			this.InitializeComponent();
		}

		internal BookDetailCollection Books { set; get; }

		public Action RefreshRequest { set; get; } = null;

		public int PaddingX { set; get; }

		public bool IsBillboard { set; get; }

		private bool IsReadList { get => this.Books.FollowAmount > 0; }
		private Thickness OutPadding { get => new Thickness(this.PaddingX, 0, this.PaddingX, 0); }
		private Visibility UserInfoVisibility { get => (!this.IsBillboard && this.IsReadList).ToVisibility(); }
		private Visibility DescriptionVisibility {
			get => (this.Books.Description != null && this.Books.Description.Length > 2).ToVisibility();
		}

		/// <summary>
		/// Navigate to detail page
		/// </summary>
		private void BookGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			var dataToPass = e.ClickedItem as BookDetail;
			if (NetworkGet.IsValidID(dataToPass.ID))
			{
				bookGrid.PrepareConnectedAnimation(Util.TO_BOOK_DETAIL, dataToPass, "bookCover");
				this._navigateItem = dataToPass;
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

		internal void RefreshPage(bool addBooks = false)
		{
			if (addBooks)
			{
				this.Books.AddBooks();
			}
			else
			{
				_ = this.Books.ReloadBooks();
			}
		}

		private void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			if (this.RefreshRequest == null)
				this.RefreshPage(true);
			else
				this.RefreshRequest();
		}
	}
}
