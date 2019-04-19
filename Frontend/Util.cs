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
    public class ReadList
    {
        public string CreateUser { set; get; }
        public string Title { set; get; }
        public string Description { set; get; }
        public BookSummaryCollection BookCollection { get; set; } = new BookSummaryCollection();

        public ReadList()
        {

        }
    }

    public class Review
    {
        public string UserName { set; get; }
        private int rating = 5;
        public int Rating {
            set
            {
                if (value <= 5 && value >= 1)
                    rating = value;
            }
            get { return rating; }
        }
        public DateTime PublishDate { set; get; }
        public string Content { set; get; }
        public string Title { set; get; }

        public Review(string name, int rating, DateTime time, string title, string content)
        {
            UserName = name;
            Rating = rating;
            PublishDate = time;
            Content = content;
            Title = title;
        }
    }

    public class BookDetail : BookSummary
    {
        public string BookDescription { get; set; }
        public string Labels { get; set; }
        public string OtherAuthors { get; set; }
        public string PublishInfo { get; set; }
        public string ISBN { get; set; }
        public double Price { get; set; }
        public int Discount { get; set; }
        public int BuyAmount { get; set; }
        public int DanmuAmount { get; set; }
        public int PreviewAmount { get; set; }
        public int ReviewAmount { get; set; }
        public int PageCount { get; set; }
        public bool CanAddWishList { get; set; }
        public bool CanAddReadList { get; set; }
        public bool CanBuy { get; set; }
        public double OverallRating { set; get; }

        public ObservableCollection<Review> Reviews { get; set; }
        public BookSummaryCollection RelatedBooks { get; set; }

        public bool finished = false;

        public const int REVIEW_ONE_TIME = 4;

        public BookDetail(BookSummary summary) : base(summary)
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

        public void GetMoreReview()
        {
            finished = false;
            Networks.RemoteGetReviews(this, this.Reviews.Count);
        }
    }

    public class BookSummary
    {
        public int BookId { get; }
        public string BookName { set; get; }
        public string BookFullName { set; get; }
        public BitmapImage BookCover { set; get; }
        public string Author { set; get; }

        public BookSummary(BookSummary book)
        {
            this.BookCover = book.BookCover;
            this.BookId = book.BookId;
            this.BookName = book.BookName;
            this.BookFullName = book.BookFullName;
            this.Author = book.Author;
        }

        public BookSummary()
        {
            this.BookId = 0;
            this.BookName = Util.WAIT_STR;
            this.BookFullName = "";
            this.BookCover = new BitmapImage(new Uri("ms-appx:///Assets/books.png"));
            this.Author = Util.WAIT_STR;

        }

        public BookSummary(string errorMsg)
        {
            this.BookId = 0;
            this.BookName = errorMsg;
            this.BookFullName = "";
            this.BookCover = new BitmapImage(new Uri("ms-appx:///Assets/books.png"));
            this.Author = "ERROR";
        }

        public readonly static BookSummary DEFAULT_BOOK = new BookSummary();

        public readonly static BookSummary TIMEOUT_BOOK = 
            new BookSummary("Timeout. Please check internet connection.");

        public BookSummary(int BookId)
        {
            this.BookId = BookId;
            this.BookName = null;
            this.BookFullName = null;
            this.BookCover = null;
            this.Author = null;
        }
    }

    public enum BookSummaryCollectionType
    {
        PersonalRecommands,
        TopBooks,
        NewBooks
    }

    public class BookSummaryCollection
    {
        public static readonly Dictionary<BookSummaryCollectionType, string> TYPE
                = new Dictionary<BookSummaryCollectionType, string>
                {
                    { BookSummaryCollectionType.PersonalRecommands, "PR" },
                    { BookSummaryCollectionType.TopBooks, "TB" },
                    { BookSummaryCollectionType.NewBooks, "NB" },
                };

        public static string GetStringType(BookSummaryCollectionType t)
        {
            if (TYPE.ContainsKey(t))
                return TYPE[t];
            else
                return "";
        }

        public BookSummaryCollection()
        {
            // do nothing
        }

        public BookSummaryCollection(BookSummaryCollection org)
        {
            this.Books = new ObservableCollection<BookSummary>(org.Books);
        }

        public BookSummaryCollection(BookSummaryCollectionType type)
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

        public const int MAX_SHOW_BOOKS = 8;

        public ObservableCollection<BookSummary> Books { set; get; } = new ObservableCollection<BookSummary>();
        
        public bool finished = false;
    }

    public class Util
    {
        public const string WAIT_STR = "Waiting...";

        public static int UserId = 0;
        public static bool isAdmin = false;
        public static MainPage main;



        public static Visibility BoolToVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility ReverseBoolToVisibility(bool notVisible)
        {
            return notVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public static string ShowStringByBool(bool visible, string format, string str1, string str2)
        {
            return visible ? string.Format(format, str1) : string.Format(format, str2);
        }

        public static string SHA256(string data)
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

        public static bool IsSubType(Type type, object obj)
        {
            return obj != null && type.IsAssignableFrom(obj.GetType());
        }
    }
}
