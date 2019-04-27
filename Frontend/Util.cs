using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Frontend
{
    internal enum ContentType
    {
        Books,
        Billboards,
        ReadLists
    }

    internal enum BooksOrderType
    {
        Recommend,
        Time,
        Rating,
        Price,
        Discount,
        ReviewAmount,
        BuyAmount,
        DanmuAmount,
        PreviewAmount,
        PageCount
    }

    internal enum BillboardsOrderType
    {
        Recommend,
        Time
    }

    internal enum ReadlistsOrderType
    {
        Recommend,
        Time,
        FollowAmount
    }

    internal enum TimeSpanType
    {
        All,
        Year,
        Month,
        Week
    }

    internal class SearchInfo: INotifyPropertyChanged
    {
        internal SearchInfo(string query)
        {
            this.QueryText = query;
            this.Init();
        }

        private readonly ObservableCollection<ComboBoxItem> BookOrders =
            new ObservableCollection<ComboBoxItem>();
        private readonly ObservableCollection<ComboBoxItem> BillboardOrders = 
            new ObservableCollection<ComboBoxItem>();
        private readonly ObservableCollection<ComboBoxItem> ReadListOrders = 
            new ObservableCollection<ComboBoxItem>();

        private BooksOrderType OrderOfBooks;
        private BillboardsOrderType OrderOfBillboards;
        private ReadlistsOrderType OrderOfReadlists;

        internal ObservableCollection<ComboBoxItem> OrderItems {
            get {
                switch (this.QueryType)
                {
                    case ContentType.Books:
                        return BookOrders;
                    case ContentType.Billboards:
                        return BillboardOrders;
                    case ContentType.ReadLists:
                        return ReadListOrders;
                    default:
                        return null;
                }
            }
        }

        internal void FromIndexSetOrder(int index)
        {
            switch (this.QueryType)
            {
                case ContentType.Books:
                    this.OrderOfBooks = (BooksOrderType)index;
                    break;
                case ContentType.Billboards:
                    this.OrderOfBillboards = (BillboardsOrderType)index;
                    break;
                case ContentType.ReadLists:
                    this.OrderOfReadlists = (ReadlistsOrderType)index;
                    break;
                default:
                    return;
            }
            this.OnPropertyChanged("OrderItems");
            this.OnPropertyChanged(null);
        }

        internal int OrderToIndex()
        {
            return this.OrderToIndex(this.QueryType);
        }

        private int OrderToIndex(ContentType type)
        {
            switch (type)
            {
                case ContentType.Books:
                    return (int)this.OrderOfBooks;
                case ContentType.Billboards:
                    return (int)this.OrderOfBillboards;
                case ContentType.ReadLists:
                    return (int)this.OrderOfReadlists;
                default:
                    return -1;
            }
        }

        internal const int MAX_PAGE_RANGE = 500;
        internal const int MIN_YEAR_RANGE = -50;
        internal const int MIN_MONTH_RANGE = -24;
        internal const int MIN_WEEK_RANGE = -24;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string name)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (name != "OrderItems")
                this.Refresh();
        }

        internal string QueryText { set; get; } = "";
        internal ContentType QueryType { set; get; }
        internal bool OrderDescend { set; get; }
        internal TimeSpanType TimeRangeType { set; get; }
        internal Range<int> TimeRange { set; get; } = new Range<int>(MIN_YEAR_RANGE, 0, MIN_YEAR_RANGE);
        internal Range<int> PageRange { set; get; } = new Range<int>(0, MAX_PAGE_RANGE, MAX_PAGE_RANGE);
        internal bool IncludeFreeBooks { set; get; }

        private readonly List<string> LabelFilters = new List<string>();

        private void Init()
        {
            foreach (BooksOrderType t in Enum.GetValues(typeof(BooksOrderType)))
                BookOrders.Add(new ComboBoxItem { Content = Util.EnumToString(t)});
            foreach (BillboardsOrderType t in Enum.GetValues(typeof(BillboardsOrderType)))
                BillboardOrders.Add(new ComboBoxItem { Content = Util.EnumToString(t) });
            foreach (ReadlistsOrderType t in Enum.GetValues(typeof(ReadlistsOrderType)))
                ReadListOrders.Add(new ComboBoxItem { Content = Util.EnumToString(t) });
            Books = new BookDetailCollection(this.ToQueryString(ContentType.Books),
                                             "Search result of " + this.QueryText, "");
            Billboards = new BookDetailCollection(this.ToQueryString(ContentType.Billboards));
            ReadLists = new BookDetailCollection(this.ToQueryString(ContentType.ReadLists));
        }

        internal void Refresh(bool add = false)
        {
            switch (this.QueryType)
            {
                case ContentType.Books:
                    if (!Books.finished)
                        return;
                    break;
                case ContentType.Billboards:
                    if (!Billboards.finished)
                        return;
                    break;
                case ContentType.ReadLists:
                    if (!ReadLists.finished)
                        return;
                    break;
                default:
                    return;
            }

            if (add)
            {
                switch (this.QueryType)
                {
                    case ContentType.Books:
                        Books.AddBooks();
                        break;
                    case ContentType.Billboards:
                        Billboards.AddBooks();
                        break;
                    case ContentType.ReadLists:
                        ReadLists.AddBooks();
                        break;
                    default:
                        return;
                }
            }
            else
            {
                switch (this.QueryType)
                {
                    case ContentType.Books:
                        Books.ReloadBooks(this.ToQueryString(), "Search result of " + this.QueryText);
                        break;
                    case ContentType.Billboards:
                        Billboards.ReloadBooks(this.ToQueryString(), "Search result of " + this.QueryText);
                        break;
                    case ContentType.ReadLists:
                        ReadLists.ReloadBooks(this.ToQueryString(), "Search result of " + this.QueryText);
                        break;
                    default:
                        return;
                }
            }
        }

        private string ToQueryString()
        {
            return this.ToQueryString(this.QueryType);
        }

        private string ToQueryString(ContentType type)
        {
            var str = "QueryText=" + QueryText;
            str += "\nResultOrder=" + this.OrderToIndex(type);
            str += "\nOrderDescend=" + OrderDescend;
            str += "\nTimeRangeType=" + (int)TimeRangeType;
            str += "\nTimeRange=" + TimeRange.ToFormalString();
            str += "\nPageRange=" + PageRange.ToFormalString();
            str += "\nLabelFilters=" + Util.ListToString<string>(LabelFilters);
            str += "\nIncludeFreeBooks=" + IncludeFreeBooks;
            return str;
        }

        internal void LabelFilterChanged()
        {
            this.LabelFilters.Clear();
            foreach (var mainLabel in Util.LABELS)
            {
                if (mainLabel.Selected)
                {
                    this.LabelFilters.Add(mainLabel.Name + "-All");
                }
                else
                {
                    var selected = mainLabel.SelectedSubs;
                    foreach (var sub in selected)
                    {
                        this.LabelFilters.Add(mainLabel.Name + "-" + sub.Name);
                    }
                }
            }
            this.OnPropertyChanged(null);
        }

        internal BookDetailCollection Books { set; get; }
        internal BookDetailCollection Billboards { set; get; }
        internal BookDetailCollection ReadLists { set; get; }

    }

    internal class Label : INotifyPropertyChanged
    {
        internal string Name { set; get; }
        internal ObservableCollection<SubLabel> AllSubs { get; set; } = new ObservableCollection<SubLabel>();

        internal ObservableCollection<SubLabel> HotSubs {
            get => new ObservableCollection<SubLabel>(AllSubs.Take(HOT_AMOUNT));
        }
        internal ObservableCollection<SubLabel> SelectedSubs {
            get {
                List<SubLabel> subs = new List<SubLabel>();
                foreach (SubLabel l in AllSubs)
                {
                    if (l.Selected)
                    {
                        subs.Add(l);
                    }
                }
                return new ObservableCollection<SubLabel>(subs);
            }
        }

        internal bool Selected { set; get; }

        private const int HOT_AMOUNT = 8;

        internal void RetriveSubs()
        {
            if (Name != null && Name.Trim().Length > 1)
            {
                Networks.RemoteGetSubLabels(this);
            }
        }

        internal Label(string name, bool selected = false)
        {
            Name = name;
            Selected = selected;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void CheckSubLabelFull()
        {
            if (this.Selected && this.SelectedSubs.Count < this.AllSubs.Count)
            {
                this.Selected = false;
                OnPropertyChanged("Selected");
            }
            else if (this.Selected && this.SelectedSubs.Count == this.AllSubs.Count)
            {
                this.Selected = true;
                OnPropertyChanged("Selected");
            }
        }

        internal void OnPropertyChanged(string name)
        {
            if (name != "Selected")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                return;
            }
            // name == Selected
            if (Selected)
            {
                foreach (SubLabel l in AllSubs)
                {
                    l.Selected = true;
                    l.OnPropertyChanged("Selected");
                }
            }
            else
            {
                bool allSubSelected = true;
                foreach (SubLabel l in AllSubs)
                {
                    if (!l.Selected)
                    {
                        allSubSelected = false;
                        break;
                    }
                }
                if (allSubSelected)
                {
                    foreach (SubLabel l in AllSubs)
                    {
                        l.Selected = false;
                        l.OnPropertyChanged("Selected");
                    }
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedSubs"));
        }
    }

    internal class SubLabel : INotifyPropertyChanged
    {
        internal string Name { set; get; }
        internal bool Selected { set; get; }

        internal Label Parent;

        internal SubLabel(string name, Label parent, bool selected = false)
        {
            Name = name;
            Selected = selected;
            this.Parent = parent;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    internal class Review
    {
        internal string UserName { set; get; }
        private int rating = 5;
        internal int Rating {
            set
            {
                if (value <= 5 && value >= 1)
                    rating = value;
            }
            get { return rating; }
        }
        internal DateTime PublishDate { set; get; }
        internal string Content { set; get; }
        internal string Title { set; get; }

        internal Review(string name, int rating, DateTime time, string title, string content)
        {
            UserName = name;
            Rating = rating;
            PublishDate = time;
            Content = content;
            Title = title;
        }
    }

    internal class BookDetail : BookSummary
    {
        internal string BookDescription { get; set; }
        internal string Labels { get; set; }
        internal string OtherAuthors { get; set; }
        internal string PublishInfo { get; set; }
        internal string ISBN { get; set; }
        internal double Price { get; set; }
        internal int Discount { get; set; }
        internal int BuyAmount { get; set; }
        internal int DanmuAmount { get; set; }
        internal int PreviewAmount { get; set; }
        internal int ReviewAmount { get; set; }
        internal int PageCount { get; set; }
        internal bool CanAddWishList { get; set; }
        internal bool CanAddReadList { get; set; }
        internal bool CanBuy { get; set; }
        internal double OverallRating { set; get; }

        internal ObservableCollection<Review> Reviews { get; set; }
        internal BookSummaryCollection RelatedBooks { get; set; }

        internal bool finished = false;

        internal const int REVIEW_ONE_TIME = 4;

        internal BookDetail(BookSummary summary) : base(summary)
        {
            BookDescription = Util.WAIT_STR;
            Labels = Util.WAIT_STR;
            OtherAuthors = Util.WAIT_STR;
            PublishInfo = Util.WAIT_STR;
            Price = OverallRating = 0;
            Discount = 100;
            ISBN = Util.WAIT_STR;
            BuyAmount = DanmuAmount = PreviewAmount = ReviewAmount = PageCount = 0;
            CanAddWishList = CanAddReadList = CanBuy = true;
            Reviews = new ObservableCollection<Review>();
            RelatedBooks = new BookSummaryCollection();
            Networks.RemoteGetBookDetail(this);
        }

        internal BookDetail(int id)
        {
            BookId = id;
        }

        internal BookDetail(string errormsg) : base(errormsg)
        {
            BookDescription = Util.WAIT_STR;
            Labels = Util.WAIT_STR;
            OtherAuthors = Util.WAIT_STR;
            PublishInfo = Util.WAIT_STR;
            Price = OverallRating = 0;
            Discount = 100;
            ISBN = Util.WAIT_STR;
            BuyAmount = DanmuAmount = PreviewAmount = ReviewAmount = PageCount = 0;
            CanAddWishList = CanAddReadList = CanBuy = true;
            Reviews = new ObservableCollection<Review>();
            RelatedBooks = new BookSummaryCollection();
        }

        internal new static readonly BookDetail TIMEOUT_BOOK =
            new BookDetail("Timeout. Please check internet connection.");

        internal void GetMoreReview()
        {
            finished = false;
            Networks.RemoteGetReviews(this, this.Reviews.Count);
        }
    }

    internal class BookSummary
    {
        internal int BookId { set; get; }
        internal string BookName { set; get; }
        internal string BookFullName { set; get; }
        internal BitmapImage BookCover { set; get; }
        internal string Author { set; get; }

        internal BookSummary(BookSummary book)
        {
            this.BookCover = book.BookCover;
            this.BookId = book.BookId;
            this.BookName = book.BookName;
            this.BookFullName = book.BookFullName;
            this.Author = book.Author;
        }

        internal BookSummary()
        {
            this.BookId = 0;
            this.BookName = Util.WAIT_STR;
            this.BookFullName = "";
            this.BookCover = new BitmapImage(new Uri("ms-appx:///Assets/books.png"));
            this.Author = Util.WAIT_STR;

        }

        internal BookSummary(string errorMsg)
        {
            this.BookId = 0;
            this.BookName = errorMsg;
            this.BookFullName = "";
            this.BookCover = new BitmapImage(new Uri("ms-appx:///Assets/books.png"));
            this.Author = "ERROR";
        }

        internal readonly static BookSummary DEFAULT_BOOK = new BookSummary();

        internal readonly static BookSummary TIMEOUT_BOOK = 
            new BookSummary("Timeout. Please check internet connection.");

        internal BookSummary(int BookId)
        {
            this.BookId = BookId;
            this.BookName = null;
            this.BookFullName = null;
            this.BookCover = null;
            this.Author = null;
        }
    }

    internal class BooklistCollection: INotifyPropertyChanged
    {
        internal ObservableCollection<BookDetailCollection> Booklists { set; get; }
            = new ObservableCollection<BookDetailCollection>();

        private const int INIT_AMOUNT = 4;
        private const int ADD_AMOUNT = 2;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly bool isBillBoard;

        internal BooklistCollection(bool isBillBoard)
        {
            this.isBillBoard = isBillBoard;
            Refresh();
        }

        internal async void Refresh(bool addMore = false)
        {
            if (!addMore)
            {
                Booklists.Clear();
            }

            int[] ids;
            if (addMore)
            {
                if (this.isBillBoard)
                    ids = await Networks.GetTopBillboardIDs(ADD_AMOUNT, Booklists.Count);
                else
                    ids = await Networks.GetTopReadListIDs(ADD_AMOUNT, Booklists.Count);
            }
            else
            {
                if (this.isBillBoard)
                    ids = await Networks.GetTopBillboardIDs(INIT_AMOUNT, 0);
                else
                    ids = await Networks.GetTopReadListIDs(INIT_AMOUNT, 0);
            }
            foreach (int id in ids)
            {
                var collection = new BookDetailCollection(
                    (this.isBillBoard ? Util.BILLBOARD_ID_QUERY : Util.READLIST_ID_QUERY)
                    + id);
                Booklists.Add(collection);
            }
        }
    }

    internal class BookDetailCollection : INotifyPropertyChanged
    {
        internal ObservableCollection<BookDetail> Books { set; get; } = new ObservableCollection<BookDetail>();
        internal string Title { set; get; }
        internal string Description { set; get; }
        internal string CreateUser { set; get; }
        internal DateTime EditTime { set; get; }
        internal int FollowAmount { set; get; }

        internal bool finished = false;

        internal string query = "";

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal BookDetailCollection(string query, string title, string content)
        {
            this.query = query;
            Title = title;
            Description = content;
            Networks.RemoteBookCollection.GetBooksFromQuery(this, this.query, Util.INIT_AMOUNT);
        }

        internal BookDetailCollection(string query)
        {
            this.query = query;
            Networks.RemoteBookCollection.GetTitleDescription(this, this.query);
            Networks.RemoteBookCollection.GetBooksFromQuery(this, this.query, Util.PREVIEW_AMOUNT);
        }

        internal void ReloadBooks(string newQuery, string newTitle)
        {
            this.Books.Clear();
            this.query = newQuery;
            this.Title = newTitle;
            this.OnPropertyChanged("Title");
            this.finished = false;
            Networks.RemoteBookCollection.GetBooksFromQuery(this, this.query, Util.PREVIEW_AMOUNT);
        }

        internal void AddBooks()
        {
            this.finished = false;
            Networks.RemoteBookCollection.GetBooksFromQuery(this, this.query, Util.ADD_AMOUNT, this.Books.Count);
        }
    }

    internal enum BookSummaryCollectionType
    {
        PersonalRecommands,
        TopBooks,
        NewBooks,
        Other
    }

    internal class BookSummaryCollection
    {
        private static readonly Dictionary<BookSummaryCollectionType, string> TYPE
                = new Dictionary<BookSummaryCollectionType, string>
                {
                    { BookSummaryCollectionType.PersonalRecommands, "PR" },
                    { BookSummaryCollectionType.TopBooks, "TB" },
                    { BookSummaryCollectionType.NewBooks, "NB" },
                };

        internal ObservableCollection<BookSummary> Books { set; get; } = new ObservableCollection<BookSummary>();

        internal bool finished = false;

        internal static string GetStringType(BookSummaryCollectionType t)
        {
            if (TYPE.ContainsKey(t))
                return TYPE[t];
            else
                return "";
        }

        internal void Refresh(BookSummaryCollectionType type)
        {
            this.Books.Clear();
            this.finished = false;
            if (type != BookSummaryCollectionType.Other)
            {
                Networks.RemoteBookCollection.GetBooksFromQuery(this, 
                    Util.DIRECT_QUERY_PREFIX + TYPE[type], Util.PREVIEW_AMOUNT);
            }
        }

        internal BookSummaryCollection()
        {
            // do nothing
        }

        internal BookSummaryCollection(BookSummaryCollectionType type)
        {
            this.Refresh(type);
        }
    }

    internal class Util
    {
        internal const int PREVIEW_AMOUNT = 8;
        internal const int INIT_AMOUNT = 14;
        internal const int ADD_AMOUNT = 6;
        internal const int RELATE_BOOK_AMOUNT = 7;

        internal const string TO_BOOK_DETAIL = "toDetail";
        internal const string FROM_BOOK_DETAIL = "fromDetail";

        internal const string DIRECT_QUERY_PREFIX = "direct-";
        internal const string BILLBOARD_ID_QUERY = "billboard_id=";
        internal const string READLIST_ID_QUERY = "readlist_id=";
        internal const string SHELF_QUERY = "shelf";

        internal const int REFRESH_RATE = 500;

        internal const string WAIT_STR = "Waiting...";

        internal static int UserId = -1;
        internal static bool isAdmin = false;
        internal static MainPage main;

        internal static ObservableCollection<Label> LABELS;

        internal static Visibility BoolToVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /*
        internal static Visibility ReverseBoolToVisibility(bool notVisible)
        {
            return notVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        internal static string ShowStringByBool(bool visible, string format, string str1, string str2)
        {
            return visible ? string.Format(format, str1) : string.Format(format, str2);
        }
        */

        internal static string SHA256(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = System.Security.Cryptography.SHA256Managed.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }

            return builder.ToString();
        }

        internal static bool IsSubType(Type type, object obj)
        {
            return obj != null && type.IsAssignableFrom(obj.GetType());
        }

        internal const int LEVEL_DataTemplate = 10;

        internal static UIElement GetParentUpto(UIElement elem, int level = 1)
        {
            if (level < 1)
                return elem;
            UIElement parent = Windows.UI.Xaml.Media.VisualTreeHelper.GetParent(elem) as UIElement;
            for (int i = 1; i < level; ++i)
            {
                parent = Windows.UI.Xaml.Media.VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return parent;
        }

        internal static string EnumToString(Enum e)
        {
            return System.Text.RegularExpressions.Regex.Replace(e.ToString("F"), @"(\p{Lu})", " $1").TrimStart();
        }

        internal static string ListToString<T>(List<T> list)
        {
            var str = "";
            foreach (T elem in list)
            {
                str += elem.ToString();
            }
            if (str.Length == 0)
            {
                return "All";
            }
            else
            {
                return "(" + str + ")";
            }
        }
    }

    internal interface IRefreshAdminInterface
    {
        void RefreshButtonPressed();

        void AdminButtonPressed(bool isChecked);
    }

    /// <summary>The Range class. From drharris on Stackoverflow</summary>
    /// <typeparam name="T">Generic parameter.</typeparam>
    internal class Range<T> where T : IComparable<T>
    {
        private readonly T inf = default;
        private readonly T minusInf = default;

        internal Range(T min, T max, T valueAsInf)
        {
            if (max.CompareTo(min) < 0)
                return;
            Minimum = min;
            Maximum = max;
            if (valueAsInf.CompareTo(max) >= 0)
            {
                this.inf = valueAsInf;
            }
            else if (valueAsInf.CompareTo(min) <= 0)
            {
                this.minusInf = valueAsInf;
            }
        }

        /// <summary>Minimum value of the range.</summary>
        internal T Minimum { get; set; }

        /// <summary>Maximum value of the range.</summary>
        internal T Maximum { get; set; }

        /// <summary>Presents the Range in readable format.</summary>
        /// <returns>String representation of the Range</returns>
        public override string ToString()
        {
            if (this.Maximum.CompareTo(this.inf) == 0 && this.inf.CompareTo(default) > 0)
                return string.Format("[{0} ~ ∞]", this.Minimum);
            else if (this.Minimum.CompareTo(this.minusInf) == 0 && this.minusInf.CompareTo(default) < 0)
                return string.Format("[-∞ ~ {0}]", this.Maximum);
            else
                return string.Format("[{0} ~ {1}]", this.Minimum, this.Maximum);
        }

        internal string ToFormalString()
        {
            if (this.Maximum.CompareTo(this.inf) == 0 && this.inf.CompareTo(default) > 0)
                return string.Format("({0},inf)", this.Minimum);
            else if (this.Minimum.CompareTo(this.minusInf) == 0 && this.minusInf.CompareTo(default) < 0)
                return string.Format("(-inf,{0})", this.Maximum);
            else
                return string.Format("({0},{1})", this.Minimum, this.Maximum);
        }

        /// <summary>Determines if the range is valid.</summary>
        /// <returns>True if range is valid, else false</returns>
        internal bool IsValid()
        {
            return this.Minimum.CompareTo(this.Maximum) <= 0;
        }

        /// <summary>Determines if the provided value is inside the range.</summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        internal bool ContainsValue(T value)
        {
            return (this.Minimum.CompareTo(value) <= 0) && (value.CompareTo(this.Maximum) <= 0);
        }

        /// <summary>Determines if this Range is inside the bounds of another range.</summary>
        /// <param name="Range">The parent range to test on</param>
        /// <returns>True if range is inclusive, else false</returns>
        internal bool IsInsideRange(Range<T> range)
        {
            return this.IsValid() && range.IsValid() && range.ContainsValue(this.Minimum)
                && range.ContainsValue(this.Maximum);
        }

        /// <summary>Determines if another range is inside the bounds of this range.</summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        internal bool ContainsRange(Range<T> range)
        {
            return this.IsValid() && range.IsValid() && this.ContainsValue(range.Minimum)
                && this.ContainsValue(range.Maximum);
        }
    }
}
