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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void ShowProgress(bool visible)
        {
            progress.IsActive = visible;
            btn.IsEnabled = !visible;
            nameBox.IsEnabled = !visible;
            passBox.IsEnabled = !visible;
        }

        private const int DELAY = 5000;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((string) btn.Content == "Press to logout")
            {
                ShowProgress(true);
                if (await Networks.RemoteLogout())
                {
                    notification.Show("Logout success", DELAY);
                    btn.Content = "Confirm";
                    ShowProgress(false);
                }
                else
                {
                    ShowProgress(false);
                    notification.Show("Please try again later", DELAY);
                }
                return;
            }

            if (nameBox.Text.Length <= 4 || passBox.Password.Length <= 6)
            {
                notification.Show("Please input valid user name & password", DELAY);
                return;
            }

            ShowProgress(true);
            var username = nameBox.Text;
            var pass = Util.SHA256(passBox.Password);
            var status = await Networks.RemoteLogin(username, pass);
            ShowProgress(false);
            switch (status)
            {
                case LoginStatus.Success:
                    nameBox.IsEnabled = false;
                    passBox.IsEnabled = false;
                    notification.Show("Login success", DELAY);
                    btn.Content = "Press to logout";
                    break;
                case LoginStatus.NoSuchUser:
                    notification.Show("No such user name or e-mail", DELAY);
                    break;
                case LoginStatus.WrongPassword:
                    notification.Show("Wrong password", DELAY);
                    break;
            }
        }

        private void Notification_Closed(object sender, Microsoft.Toolkit.Uwp.UI.Controls.InAppNotificationClosedEventArgs e)
        {
            if ((string)notification.Content == "Login success")
            {
                Util.main.NavigateToHomeAndShowMine(true);
            }
            else if ((string)notification.Content == "Logout success")
            {
                Util.main.NavigateToHomeAndShowMine(false);
            }
        }
    }
}
