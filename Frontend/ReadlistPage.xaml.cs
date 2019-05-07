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

        internal static string DateFollow(DateTime date, int followers, string user)
        {
            return string.Format("Created by: {0}\t at:\t{0}\t with {1} followers",
                                 user, date.ToShortDateString(), followers);
        }

        internal static string DateFollow(DateTime date, int followers)
        {
            return string.Format("Last edit at:\t{0}\n#Followers:\t{1}",
                                 date.ToShortDateString(), followers);
        }
    }
}
