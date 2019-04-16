using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Frontend
{
    public class BookSummary
    {
        public int BookId { get; }
        public string BookName { set; get; }
        public BitmapImage BookCover { set; get; }
        public string Author { set; get; }


        public BookSummary()
        {
            this.BookId = 0;
            this.BookName = "Waiting";
            this.BookCover = new BitmapImage(new Uri("ms-appx:///Assets/books.png"));
            this.Author = "Waiting";
        }

        public readonly static BookSummary DEFAULT_BOOK = new BookSummary();

        public BookSummary(int BookId)
        {
            this.BookId = BookId;
            this.BookName = null;
            this.BookCover = null;
            this.Author = null;
        }


        private async Task<bool> GetBookSummary(int TimeoutInms)
        {
            return await Networks.RemoteGetBookSummary(this);
        }
    }

    public enum BookSummaryCollectionType
    {
        PersonalRecommands,
        TopBooks,
        NewBooks,
        TopReadlists,
        NewReadlists
    }

    public class BookSummaryCollection
    {
        public BookSummaryCollection(BookSummaryCollectionType type)
        {
            Books = new ObservableCollection<BookSummary>
            {
                BookSummary.DEFAULT_BOOK
            };
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
                case BookSummaryCollectionType.TopReadlists:
                    Networks.RemoteBookCollection.GetTopReadlists(this);
                    break;
                case BookSummaryCollectionType.NewReadlists:
                    Networks.RemoteBookCollection.GetNewReadlists(this);
                    break;
            }
        }

        public ObservableCollection<BookSummary> Books { set; get; }

        public bool FinishFlag = false;

        public async Task<bool> WaitTillFinish()
        {
            while (!this.FinishFlag)
            {
                await Task.Delay(50);
            }
            return true;
        }
    }

    public class Util
    {
        public static int UserId = 0;
        public static bool isAdmin = false;
        public static MainPage main;

        public static Visibility ConvertFromBool(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
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
    }
}
