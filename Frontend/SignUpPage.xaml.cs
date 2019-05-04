﻿using Microsoft.Toolkit.Uwp.UI.Controls;
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
    public sealed partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void ShowProgress(bool visible)
        {
            progress.IsActive = visible;
            btn.IsEnabled = !visible;
            nameBox.IsEnabled = !visible;
            emailBox.IsEnabled = !visible;
            passBox.IsEnabled = !visible;
            confirmPassBox.IsEnabled = !visible;
        }

        private const int DELAY = 5000;

        private async void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (passBox.Password != confirmPassBox.Password)
            {
                notification.Show("Password and confirm password are dismatched", DELAY);
                return;
            }
            if (nameBox.Text.Length <= 4 || passBox.Password.Length <= 6 || !emailBox.Text.Contains("@"))
            {
                notification.Show("Please input valid user name, e-mail or password", DELAY);
                return;
            }

            ShowProgress(true);
            var pass = Util.SHA256(passBox.Password);
            var status = await NetworkSet.SignUp(nameBox.Text, emailBox.Text, pass);
            ShowProgress(false);
            if (status)
            {
                notification.Show("Sign up success", DELAY);
            }
            else
            {
                notification.Show("Confliction occured.\r\n" +
                                  "Please try again with different user name and/or e-mail.", DELAY);
            }
        }

        private void Notification_Closed(object sender, InAppNotificationClosedEventArgs e)
        {
            if (notification.Content as string == "Sign up success")
            Util.main.NavigateToHomeAndShowMine(false, e.DismissKind == InAppNotificationDismissKind.User);
        }
    }
}