using System;
using System.Collections.Generic;
using System.ComponentModel;
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


namespace Frontend
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BooklistPage : Page, IRefreshAdminInterface
    {
        public BooklistPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private QueryObject query = new QueryObject();
        private string title = "";
        private string description = "";
        private bool IsBillboard { get => !query.IsBillboard.HasValue || query.IsBillboard.Value; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var para = ((string title, string description, QueryObject query))e.Parameter;
            if (this.title == para.title && this.description == para.description && this.query == para.query)
                return;
            this.title = para.title;
            this.description = para.description;
            this.query = para.query;
            bookCollection = new CustomControls.BookCollectionControl()
            {
                PaddingX = 120,
                IsBillboard = this.IsBillboard
            };
            bookCollection.Books = new BookDetailCollection(this.query, this.title, this.description);
        }

        public void RefreshButtonPressed()
        {
            bookCollection.RefreshPage();
        }

        public void AdminButtonPressed(bool isChecked)
        {
            // do nothing
        }

        internal static string PriceDiscount(double price, int discount)
        {
            if (discount == 100)
            {
                return string.Format("Price:\t{0:C2}", price);
            }
            else
            {
                return string.Format("Price:\t{0:C2} ({1}% OFF)", price, 100 - discount);
            }
        }
    }
}
