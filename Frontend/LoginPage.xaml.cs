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
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void ShowProgress(bool visible)
        {
            progress.IsActive = visible;
            btn.IsEnabled = !visible;
            nameBox.IsEnabled = !visible;
            passBox.IsEnabled = !visible;
            //lastRow.Height = new GridLength(lastRow.Height.Value * (visible ? 2 : 0.5),
            //    lastRow.Height.GridUnitType);
        }

        private const int DELAY = 2000;

        /// <summary>
        /// Click login confirm button
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((string) btn.Content == "Press to logout")
            {
                ShowProgress(true);
                if (await Networks.RemoteLogout())
                {
                    btn.Content = "Logout success";
                    await System.Threading.Tasks.Task.Delay(DELAY);
                    btn.Content = "Confirm";
                    ShowProgress(false);
                    Util.main.NavigateToHomeAndShowMine(false);
                }
                else
                {
                    progress.IsActive = false;
                    btn.Content = "Please try again later";
                    await System.Threading.Tasks.Task.Delay(DELAY);
                    btn.Content = "Press to logout";
                    btn.IsEnabled = true;
                }
                return;
            }

            if (nameBox.Text.Length <= 4 || passBox.Password.Length <= 6)
            {
                var orgText = nameBox.Text;
                nameBox.Text = "Please input valid user name & password";
                nameBox.IsEnabled = false;
                await System.Threading.Tasks.Task.Delay(DELAY);
                nameBox.IsEnabled = true;
                nameBox.Text = orgText;
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
                    btn.Content = "Success";
                    btn.IsEnabled = false;
                    nameBox.IsEnabled = false;
                    passBox.IsEnabled = false;
                    await System.Threading.Tasks.Task.Delay(DELAY);
                    btn.Content = "Press to logout";
                    btn.IsEnabled = true;
                    Util.main.NavigateToHomeAndShowMine(true);
                    break;
                case LoginStatus.NoSuchUser:
                    var orgText = nameBox.Text;
                    nameBox.Text = "Wrong user name or e-mail";
                    nameBox.IsEnabled = false;
                    await System.Threading.Tasks.Task.Delay(DELAY);
                    nameBox.IsEnabled = true;
                    nameBox.Text = orgText;
                    break;
                case LoginStatus.WrongPassword:
                    orgText = nameBox.Text;
                    nameBox.Text = "Wrong password";
                    nameBox.IsEnabled = false;
                    await System.Threading.Tasks.Task.Delay(DELAY);
                    nameBox.IsEnabled = true;
                    nameBox.Text = orgText;
                    break;
            }
        }
    }
}
