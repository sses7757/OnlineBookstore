using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend
{
    public enum LoginStatus
    {
        Success,
        NoSuchUser,
        WrongPassword
    }
    public class Networks
    {
        public const string REMOTE_IP = "10.20.30.40";

        public static async Task<LoginStatus> RemoteLogin(string username, string encodedPassword)
        {
            // TODO
            await Task.Delay(4000);
            Util.UserId = 1111;
            Util.isAdmin = true;
            return LoginStatus.Success;
        }

        public static async Task<bool> RemoteLogout()
        {
            // TODO
            await Task.Delay(4000);
            Util.UserId = 0;
            Util.isAdmin = false;
            return true;
        }

        public static async Task<bool> RemoteGetBookSummary(BookSummary book)
        {
            if (book.BookId <= 0)
            {
                throw new ArgumentNullException("Book id wrong");
            }
            // TODO
            await Task.Delay(500);
            book.BookCover = new Windows.UI.Xaml.Media.Imaging.BitmapImage(
                new Uri("ms-appx:///Assets/tempBook.png"));
            book.BookName = "Test book name -- ABC";
            book.Author = "Test author name";
            return true;
        }

        public class RemoteBookCollection
        {
            private const int delay = 1000;

            public static async void GetPersonalRecommands(BookSummaryCollection collection)
            {
                // TODO
                await Task.Delay(delay);
                foreach (int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    BookSummary book = new BookSummary(i);
                    while (! await RemoteGetBookSummary(book)) { }
                    collection.Books.Add(book);
                }
                collection.FinishFlag = true;
                collection.Books.Remove(BookSummary.DEFAULT_BOOK);
            }

            public static async void GetTopBooks(BookSummaryCollection collection)
            {
                // TODO
                await Task.Delay(delay);
                foreach (int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    BookSummary book = new BookSummary(i);
                    while (!await RemoteGetBookSummary(book)) { }
                    collection.Books.Add(book);
                }
                collection.FinishFlag = true;
                collection.Books.Remove(BookSummary.DEFAULT_BOOK);
            }

            public static async void GetNewBooks(BookSummaryCollection collection)
            {
                // TODO
                await Task.Delay(delay);
                foreach (int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    BookSummary book = new BookSummary(i);
                    while (!await RemoteGetBookSummary(book)) { }
                    collection.Books.Add(book);
                }
                collection.FinishFlag = true;
                collection.Books.Remove(BookSummary.DEFAULT_BOOK);
            }

            public static async void GetTopReadlists(BookSummaryCollection collection)
            {
                // TODO
                await Task.Delay(delay);
                foreach(int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    BookSummary book = new BookSummary(i);
                    while (!await RemoteGetBookSummary(book)) { }
                    collection.Books.Add(book);
                }
                collection.FinishFlag = true;
                collection.Books.Remove(BookSummary.DEFAULT_BOOK);
            }

            public static async void GetNewReadlists(BookSummaryCollection collection)
            {
                // TODO
                await Task.Delay(delay);
                foreach (int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    BookSummary book = new BookSummary(i);
                    while (!await RemoteGetBookSummary(book)) { }
                    collection.Books.Add(book);
                }
                collection.FinishFlag = true;
                collection.Books.Remove(BookSummary.DEFAULT_BOOK);
            }
        }
    }
}
