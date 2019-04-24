using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Frontend
{
    internal class Label : INotifyPropertyChanged
    {
        internal string Name { set; get; }
        internal ObservableCollection<string> AllSubs { get; set; } = new ObservableCollection<string>();

        internal ObservableCollection<string> HotSubs {
            get => new ObservableCollection<string>(AllSubs.Take(HOT_AMOUNT));
        }

        private const int HOT_AMOUNT = 8;

        internal void RetriveSubs()
        {
            if (Name != null && Name.Trim().Length > 1)
            {
                Networks.RemoteGetSubLabels(this);
            }
        }

        internal Label(string name)
        {
            this.Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    internal class ReadList
    {
        internal string CreateUser { set; get; }
        internal string Title { set; get; }
        internal string Description { set; get; }
        internal BookSummaryCollection BookCollection { get; set; } = new BookSummaryCollection();

        internal ReadList()
        {

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

    internal class BookDetailCollection
    {
        internal ObservableCollection<BookDetail> Books { set; get; } = new ObservableCollection<BookDetail>();
        internal string Title { set; get; }
        internal string Description { set; get; }

        internal bool finished = false;

        private string query = "";

        internal BookDetailCollection(string query, string title, string content)
        {
            this.query = query;
            Title = title;
            Description = content;
            Networks.RemoteBookCollection.GetBooksFromQuery(this, query, INIT_AMOUNT);
        }

        internal BookDetailCollection(string query)
        {
            this.query = query;
            Networks.RemoteBookCollection.GetTitleDescription(this, query);
            Networks.RemoteBookCollection.GetBooksFromQuery(this, query, PREVIEW_AMOUNT);
        }

        internal const int PREVIEW_AMOUNT = 8;
        internal const int INIT_AMOUNT = 14;
        internal const int ADD_AMOUNT = 6;

        internal void AddBooks()
        {
            this.finished = false;
            Networks.RemoteBookCollection.GetBooksFromQuery(this, query, ADD_AMOUNT, this.Books.Count);
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
        internal const string DIRECT_QUERY_PREFIX = "direct-";

        internal static readonly Dictionary<BookSummaryCollectionType, string> TYPE
                = new Dictionary<BookSummaryCollectionType, string>
                {
                    { BookSummaryCollectionType.PersonalRecommands, "PR" },
                    { BookSummaryCollectionType.TopBooks, "TB" },
                    { BookSummaryCollectionType.NewBooks, "NB" },
                };

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
            switch (type)
            {
                case BookSummaryCollectionType.PersonalRecommands:
                    Networks.RemoteBookCollection.GetPersonalRecommands(this);
                    break;
                case BookSummaryCollectionType.TopBooks:
                    Networks.RemoteBookCollection.GetTopBooks(this);
                    break;
                case BookSummaryCollectionType.NewBooks:
                    Networks.RemoteBookCollection.GetNewBooks(this);
                    break;
                default:
                    this.finished = true;
                    break;
            }
        }

        internal BookSummaryCollection()
        {
            // do nothing
        }

        internal BookSummaryCollection(BookSummaryCollection org)
        {
            this.Books = new ObservableCollection<BookSummary>(org.Books);
        }

        internal BookSummaryCollection(BookSummaryCollectionType type)
        {
            switch (type)
            {
                case BookSummaryCollectionType.PersonalRecommands:
                    Networks.RemoteBookCollection.GetPersonalRecommands(this);
                    break;
                case BookSummaryCollectionType.TopBooks:
                    Networks.RemoteBookCollection.GetTopBooks(this);
                    break;
                case BookSummaryCollectionType.NewBooks:
                    Networks.RemoteBookCollection.GetNewBooks(this);
                    break;
                default:
                    break;
            }
        }

        internal const int MAX_SHOW_BOOKS = 8;

        internal ObservableCollection<BookSummary> Books { set; get; } = new ObservableCollection<BookSummary>();
        
        internal bool finished = false;
    }

    internal class Util
    {
        internal const string TO_BOOK_DETAIL = "toDetail";
        internal const string FROM_BOOK_DETAIL = "fromDetail";

        internal const int REFRESH_RATE = 500;

        internal const string WAIT_STR = "Waiting...";

        internal static int UserId = 0;
        internal static bool isAdmin = false;
        internal static MainPage main;



        internal static Visibility BoolToVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        internal static Visibility ReverseBoolToVisibility(bool notVisible)
        {
            return notVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        internal static string ShowStringByBool(bool visible, string format, string str1, string str2)
        {
            return visible ? string.Format(format, str1) : string.Format(format, str2);
        }

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
    }

    internal interface IRefreshAdminInterface
    {
        void RefreshButtonPressed();

        void AdminButtonPressed(bool isChecked);
    }
}
