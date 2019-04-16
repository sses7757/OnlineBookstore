using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("home", typeof(HomePage)),
            ("readlist", typeof(ReadlistPage)),
            ("billboard", typeof(BillboardPage)),
            ("bookshelf", typeof(BookshelfPage)),
            ("mydanmu", typeof(MyDanmuPage)),
            ("myreadlist", typeof(MyReadlistPage)),
            ("mywishlist", typeof(MyWishlistPage)),
            ("login", typeof(LoginPage)),
        };

        private void ShowAdmin(bool visible)
        {
            var v = Util.ConvertFromBool(visible);
            TopSeparator.Visibility = v;
            ToggleAdmin.Visibility = v;
        }

        private void ShowMyAccount(bool visible)
        {
            var v = Util.ConvertFromBool(visible);
            BookshelfBtn.Visibility = v;
            MyDanmuBtn.Visibility = v;
            MyReadlistBtn.Visibility = v;
            MyWishlistBtn.Visibility = v;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        public void NavigateToHomeAndShowMine(bool show)
        {
            if (show)
            {
                NavView.SelectedItem = NavView.MenuItems[1];
                ShowMyAccount(true);
            }
            else
            {
                ShowMyAccount(false);
                NavView.SelectedItem = NavView.MenuItems[NavView.MenuItems.Count - 1];
            }
        }

        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo, bool Override = true)
        {
            // show admin toggle or not
            ShowAdmin(Util.isAdmin);

            Type _page = null;
            if (navItemTag == "settings")
            {
                _page = typeof(SettingPage);
            }
            else
            {
                var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                _page = item.Page;
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !Equals(preNavPageType, _page))
            {
                //Continuum, Common, DrillIn for go inside, Entrance, Slide, Suppress
                if (Override) {
                    transitionInfo = new SlideNavigationTransitionInfo()
                    { Effect = SlideNavigationTransitionEffect.FromRight };
                }
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Add keyboard accelerators for backwards navigation.
            var goBack = new KeyboardAccelerator { Key = VirtualKey.GoBack };
            goBack.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(goBack);
            // ALT routes here
            var altLeft = new KeyboardAccelerator
            {
                Key = VirtualKey.Left,
                Modifiers = VirtualKeyModifiers.Menu
            };
            altLeft.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(altLeft);

            Util.main = this;

            NavView_Navigate("home", new EntranceNavigationTransitionInfo(), false);
            ShowAdmin(false);
            ShowMyAccount(false);
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                 NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(SettingPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
                WelcomeLabel1.Text = "Settings";
                WelcomeLabel2.Text = "";
                return;
            }
            else if (ContentFrame.SourcePageType == typeof(LoginPage) || ContentFrame.SourcePageType == typeof(HomePage))
            {
                WelcomeLabel1.Text = "Welcome to";
                WelcomeLabel2.Text = " BookHub";
            }
            else if (ContentFrame.SourcePageType != null)
            {
                WelcomeLabel1.Text = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
                WelcomeLabel2.Text = "";
            }
            var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);
            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First(n => n.Tag.Equals(item.Tag));
        }
    }
}
