using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BillboardPage : Page, IRefreshAdminInterface
    {
        public BillboardPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            Billboards = new BillboardCollection();
            WaitLoading();
        }

        private async void WaitLoading()
        {
            while (Billboards.Billboards.Count == 0)
                await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);

            while (true)
            {
                var load = false;
                foreach (var v in Billboards.Billboards)
                {
                    if (!v.finished)
                    {
                        load = true;
                        break;
                    }
                }
                if (!load)
                {
                    loadingControl.IsLoading = false;
                    break;
                }
                else
                {
                    await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE);
                    Billboards.OnPropertyChanged("Billboards");
                }
            }
            await System.Threading.Tasks.Task.Delay(Util.REFRESH_RATE * 2);
            Billboards.OnPropertyChanged("Billboards");
        }

        internal BillboardCollection Billboards { set; get; }

        private void Refresh(bool add)
        {
            if (!loadingControl.IsLoading)
            {
                Billboards.Refresh(add);
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

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Book_Pointed(object sender, PointerRoutedEventArgs e)
        {
            
        }
    }
}
