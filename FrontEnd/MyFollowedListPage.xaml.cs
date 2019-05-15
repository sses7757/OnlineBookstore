using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class MyFollowedListPage : Page, IRefreshAdminInterface
	{
		public MyFollowedListPage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;

			listControl.Booklist = new BooklistCollection(false);
			listControl.WaitLoading();
		}

		public void RefreshButtonPressed()
		{
			listControl.Refresh();
		}

		public void AdminButtonPressed(bool isChecked)
		{
			// do nothing
		}
	}
}
