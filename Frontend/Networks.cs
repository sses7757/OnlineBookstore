using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                new Uri("https://images-na.ssl-images-amazon.com/images/I/51JVLQdducL._SX392_BO1,204,203,200_.jpg"));
                //new Uri("ms-appx:///Assets/tempBook.png"));
            book.BookName = "An Introduction to Thermal Physics";
            book.BookFullName = "An Introduction to Thermal Physics " +
                "(Translation Version by Database System Principle Team 309)";
            book.Author = "Daniel V. Schroeder";
            return true;
        }

        public static async void RemoteGetBookDetail(BookDetail book)
        {
            // TODO get book details Util.UserId
            await Task.Delay(1000);
            book.BookDescription = "The authors present the complete guide to ANSI standard C language programming." +
                " Written by the developers of C, this new version helps readers keep up with the finalized ANSI " +
                "standard for C while showing how to take advantage of C's rich set of operators, economy of expression, " +
                "improved control flow, and data structures. The 2/E has been completely rewritten with additional" +
                " examples and problem sets to clarify the implementation of difficult language constructs. " +
                "For years, C programmers have let K&R guide them to building well-structured and efficient programs." +
                " Now this same help is available to those working with ANSI compilers. Includes detailed coverage of" +
                " the C language plus the official C language reference manual for at-a-glance help with syntax notation," +
                " declarations, ANSI changes, scope rules, and the list goes on and on.";
            await Task.Delay(100);
            book.Labels = "Science-Physics-Thermal physics";
            await Task.Delay(100);
            book.OtherAuthors = "Database System Principle Team 309 (translators)";
            await Task.Delay(100);
            book.PublishInfo = "Pearson / 1999-08-28 / 1st edition";
            await Task.Delay(100);
            book.Price = 48.67;
            book.Discount = 85;
            await Task.Delay(100);
            book.ISBN = "978-0131103627";
            await Task.Delay(100);
            book.BuyAmount = 120;
            book.DanmuAmount = 200;
            book.PreviewAmount = 1005;
            book.ReviewAmount = 25;
            book.PageCount = 462;
            book.OverallRating = 4.6;
            book.CanAddReadList = true;
            book.CanAddWishList = book.CanBuy = false;
            RemoteGetReviews(book);
            await RemoteBookCollection.GetRelatedBooks(book.RelatedBooks, book.BookId);
            book.finished = true;
        }

        public static async void RemoteGetReviews(BookDetail book, int from = 0,
                                                        int count = BookDetail.REVIEW_ONE_TIME)
        {
            // TODO get book reviews (from -> from + count)
            await Task.Delay(1000);
            for (int i = from; i < count - 1; ++i)
            {
                book.Reviews.Add(new Review("Steven", 5, new DateTime(2019, 3, 24, 0, 45, 4), "A Fansinating Book", "I'm not new to programming; in fact I've been doing it professionally for the past decade. Although I've played around in quite a few different languages, most of my work over the last 6 years has been in .NET (C# mainly). I have always had an interest in C because I love its simplicity. Also, it's a language which brings one closer to the machine, stripping away many of the abstractions that higher level languages provide. Higher level languages (such as Java, C#, Python, etc.) are massive and powerful with HUGE frameworks, but I'm attracted to simple things."));
                await Task.Delay(500);
            }
            book.Reviews.Add(new Review("Anis", 1, DateTime.Now, "Disappointed", "Print Type is very bad. Looks like old news paper printed on 80's. Too much description, may be good for beginners students but not for you if want to understand the concept of C Pointer, Structure, Union, etc. in few lines. I found may online tutorials better than this book. Just read the book less - than hours and returned; Paid $7.53 shipping fee....bad."));
            await Task.Delay(500);
            if (from != 0)
            {
                book.finished = true;
            }
        }

        public class RemoteBookCollection
        {
            private const int delay = 1000;

            private static async Task<bool> AddBookSummary(BookSummaryCollection collection, int bookId, long timeout = 1000)
            {
                bool flag = true;
                var timer = new Stopwatch();
                timer.Start();
                BookSummary book = new BookSummary(bookId);
                while (!await RemoteGetBookSummary(book))
                {
                    if (timer.ElapsedMilliseconds > timeout)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    collection.Books.Add(book);
                return flag;
            }

            private static void SetBookSummaryCollection(BookSummaryCollection collection, bool flag)
            {
                if (!flag)
                {
                    collection.Books.Add(BookSummary.TIMEOUT_BOOK);
                }
                collection.finished = true;
            }

            public static async void GetPersonalRecommands(BookSummaryCollection collection)
            {
                // TODO get book ids
                await Task.Delay(delay);
                bool flag = true;
                foreach (int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    flag = await AddBookSummary(collection, i);
                    if (!flag)
                        break;
                }
                SetBookSummaryCollection(collection, flag);
            }

            public static async void GetTopBooks(BookSummaryCollection collection)
            {
                // TODO get book ids
                await Task.Delay(delay);
                bool flag = true;
                foreach (int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    flag = await AddBookSummary(collection, i);
                    if (!flag)
                        break;
                }
                SetBookSummaryCollection(collection, flag);
            }

            public static async void GetNewBooks(BookSummaryCollection collection)
            {
                // TODO get book ids
                await Task.Delay(delay);
                bool flag = true;
                foreach (int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    flag = await AddBookSummary(collection, i);
                    if (!flag)
                        break;
                }
                SetBookSummaryCollection(collection, flag);
            }

            public static async Task<bool> GetRelatedBooks(BookSummaryCollection collection, int relatedBookId)
            {
                // TODO get book ids
                await Task.Delay(delay);
                bool flag = true;
                foreach (int i in new int[] { 1, 2, 3, 4, 5, 6, 7, 8 })
                {
                    flag = await AddBookSummary(collection, i);
                    if (!flag)
                        break;
                }
                SetBookSummaryCollection(collection, flag);
                return flag;
            }
        }
    }
}
