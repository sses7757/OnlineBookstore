using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend
{

    public class QueryObject
    {
        public string Type { set; get; }
        public string UserName { set; get; }
        public string EncodedPassowrd { set; get; }
        public string MainLabel { set; get; }
        public int? BookId { set; get; }
        public bool? IsBillboard { set; get; }
        public int? BookListId { set; get; }
        public int? ReviewId { set; get; }
        public int? DanmuId { set; get; }
        public int? From { set; get; }
        public int? Count { set; get; }

        public int? SearchType { set; get; }
        public string DirectQuery { set; get; }
        public string QueryText { set; get; }
        public bool? OrderDescend { set; get; }
        public int? Order { set; get; }
        public int? TimeRangeType { set; get; }
        public int[] TimeRange { set; get; }
        public int[] PageRange { set; get; }
        public string[] LabelFilters { set; get; }
        public bool? IncludeFreeBooks { set; get; }

        public int? Page { set; get; }

        public QueryObject() { }

        public QueryObject(string type)
        {
            Type = type;
        }

        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                //DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(this, settings);
        }
    }

    public class NetworkGet
    {
        public const string REMOTE_IP = "10.20.30.40";

        internal static bool IsValidID(int id)
        {
            return id >= 0;
        }

        public enum LoginStatus
        {
            Success,
            NoSuchUser,
            WrongPassword
        }

        public static async Task<LoginStatus> Login(string UserName, string EncodedPassword)
        {
            var query = new QueryObject("Login")
            {
                UserName = UserName,
                EncodedPassowrd = EncodedPassword
            };
            var json = query.ToString();
            // TODO
            await Task.Delay(4000);
            Util.UserId = 1111;
            Util.isAdmin = true;
            return LoginStatus.Success;
        }
        public static async Task<string[]> GetMainLabels()
        {
            var query = new QueryObject("GetMainLabels")
            {
            };
            var json = query.ToString();
            // TODO
            await Task.Delay(1000);
            return new string[] { "Arts",
                                  "Biographies",
                                  "Business",
                                  "Calendars",
                                  "Sciences",
                                  "Bibles"
            };
        }

        public static async void GetSubLabels(Label label)
        {
            var query = new QueryObject("GetMainLabels")
            {
                MainLabel = label.Name
            };
            var json = query.ToString();
            // TODO
            await Task.Delay(1000);
            for (int i = 0; i < 15; ++i)
            {
                label.AllSubs.Add(new SubLabel("AA", label));
                label.OnPropertyChanged("HotSubs");
                await Task.Delay(200);
            }
        }

        public static async Task<bool> GetBookSummary(BookSummary book)
        {
            if (!IsValidID(book.BookId))
            {
                Console.Error.WriteLine("Book id wrong");
                return false;
            }
            var query = new QueryObject("GetBookSummary")
            {
                BookId = book.BookId
            };
            var json = query.ToString();
            // TODO
            await Task.Delay(Util.REFRESH_RATE);
            book.BookCover = new Windows.UI.Xaml.Media.Imaging.BitmapImage(
                new Uri("https://images-na.ssl-images-amazon.com/images/I/51JVLQdducL._SX392_BO1,204,203,200_.jpg"));
                //new Uri("ms-appx:///Assets/tempBook.png"));
            book.BookName = "An Introduction to Thermal Physics";
            book.BookFullName = "An Introduction to Thermal Physics " +
                "(Translation Version by Database System Principle Team 309)";
            book.Author = "Daniel V. Schroeder";
            return true;
        }

        public static async Task<bool> GetBookQuasiDetail(BookDetail book)
        {
            if (!IsValidID(book.BookId))
            {
                Console.Error.WriteLine("Book id wrong");
                return false;
            }
            var query = new QueryObject("GetBookQuasiDetail")
            {
                BookId = book.BookId
            };
            var json = query.ToString();
            // TODO only two level label
            await Task.Delay(Util.REFRESH_RATE);
            book.BookCover = new Windows.UI.Xaml.Media.Imaging.BitmapImage(
                new Uri("https://images-na.ssl-images-amazon.com/images/I/51JVLQdducL._SX392_BO1,204,203,200_.jpg"));
            //new Uri("ms-appx:///Assets/tempBook.png"));
            book.BookName = "An Introduction to Thermal Physics";
            book.BookFullName = "An Introduction to Thermal Physics " +
                "(Translation Version by Database System Principle Team 309)";
            book.Author = "Daniel V. Schroeder";
            await Task.Delay(100);
            book.Labels = "Science-Physics";
            await Task.Delay(100);
            book.Price = 48.67;
            book.Discount = 85;
            await Task.Delay(100);
            book.OverallRating = 4.6;
            return true;
        }

        public static async Task<bool> GetBookDetail(BookDetail book)
        {
            var query = new QueryObject("GetBookDetail")
            {
                BookId = book.BookId
            };
            var json = query.ToString();
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
            await book.RelatedBooks.Reload();
            await GetReviewContents(book);
            book.finished = true;
            return true;
        }

        internal static async Task<bool> GetReviewContents(BookDetail book, bool setFinish = false, int from = 0,
                                                           int count = Util.REVIEW_AMOUNT_ONE_TIME)
        {
            var reviewIds = await GetBookReviews(book.BookId, from, count);
            foreach (var rid in reviewIds)
            {
                var review = new Review(rid);
                await GetReview(review);
                book.Reviews.Add(review);
            }
            if (setFinish)
                book.finished = true;
            return true;
        }

        public static async Task<int[]> GetBookReviews(int bookId, int from, int count)
        {
            var query = new QueryObject("GetBookReviews")
            {
                BookId = bookId,
                From = from,
                Count = count
            };
            var json = query.ToString();
            // TODO get book reviews (from -> from + count)
            await Task.Delay(100);
            var ids = new List<int>();
            for (int i = from; i < count + from; ++i)
            {
                ids.Add(i + 479);
            }
            return ids.ToArray();
        }

        public static async Task<bool> GetReview(Review review)
        {
            var query = new QueryObject("GetReview")
            {
                ReviewId = review.ID
            };
            var json = query.ToString();
            // TODO get review content from id
            await Task.Delay(500);
            if (new Random().Next(0, 6) < 5)
            {
                review.UserName = "Steven";
                review.PublishDate = new DateTime(2019, 3, 24, 0, 45, 4);
                review.Rating = 5;
                review.Title = "A Fansinating Book";
                review.Content = "I'm not new to programming; in fact " +
                    "I've been doing it professionally for the past decade." +
                    " Although I've played around in quite a few different languages, " +
                    "most of my work over the last 6 years has been in .NET (C# mainly)." +
                    " I have always had an interest in C because I love its simplicity." +
                    " Also, it's a language which brings one closer to the machine, " +
                    "stripping away many of the abstractions that higher level languages provide." +
                    " Higher level languages (such as Java, C#, Python, etc.) are massive and " +
                    "powerful with HUGE frameworks, but I'm attracted to simple things.";
            }
            else
            {
                review.UserName = "Anis";
                review.PublishDate = DateTime.Now;
                review.Rating = 1;
                review.Title = "Disappointed";
                review.Content = "Print Type is very bad. Looks like old news paper printed on 80's." +
                    " Too much description, may be good for beginners students but not for you if" +
                    " want to understand the concept of C Pointer, Structure, Union, etc. in few lines." +
                    " I found may online tutorials better than this book. Just read the book less -" +
                    " than hours and returned; Paid $7.53 shipping fee....bad.";
            }
            return true;
        }

        public static async Task<int[]> GetBookListBooks(bool isBillboard, int id, int from = 0,
                                                         int count = Util.PREVIEW_AMOUNT)
        {
            var query = new QueryObject("GetBookListBooks")
            {
                IsBillboard = isBillboard,
                BookListId = id,
                From = from,
                Count = count
            };
            var json = query.ToString();
            // TODO get book reviews (from -> from + count)
            await Task.Delay(100);
            var ids = new List<int>();
            for (int i = from; i < (count == int.MaxValue ? 12 : count) + from; ++i)
            {
                ids.Add(i + 1657);
            }
            return ids.ToArray();
        }

        public static async Task<int[]> GetShelfBooks()
        {
            var query = new QueryObject("GetShelfBooks")
            {
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(100);
            var ids = new List<int>();
            for (int i = 0; i < 18; ++i)
            {
                ids.Add(i + 768);
            }
            return ids.ToArray();
        }

        private const int delay = 1000;

        public static async Task<int[]> GetFromQuery(QueryObject query, int from = 0, int count = int.MaxValue)
        {
            query.Type = "GetFromQuery";
            query.From = from;
            query.Count = count;
            var json = query.ToJson();
            // TODO get ids
            await Task.Delay(delay);
            List<int> ids = new List<int>();
            for (int i = from + 1; i <= (count == int.MaxValue ? 18 : count) + from; ++i)
            {
                ids.Add(i + 235);
            }
            return ids.ToArray();
        }

        internal readonly static QueryObject NewBooks =
            new QueryObject()
            {
                SearchType = 0,
                OrderDescend = true,
                Order = 1
            };

        internal readonly static QueryObject TopBooks =
            new QueryObject()
            {
                SearchType = 0,
                OrderDescend = true,
                Order = 6
            };

        internal readonly static QueryObject PersonalRecommend =
            new QueryObject()
            {
                SearchType = 0,
                OrderDescend = false,
                Order = 0
            };

        internal static QueryObject BillboardRecommend =
            new QueryObject()
            {
                SearchType = 1,
                OrderDescend = false,
                Order = 0
            };

        internal readonly static QueryObject ReadListRecommend =
            new QueryObject()
            {
                SearchType = 2,
                OrderDescend = false,
                Order = 0
            };

        internal static QueryObject BillboardTop =
            new QueryObject()
            {
                SearchType = 1,
                OrderDescend = true,
                Order = 1
            };

        internal readonly static QueryObject ReadListTop =
            new QueryObject()
            {
                SearchType = 2,
                OrderDescend = false,
                Order = 2
            };

        internal static async Task<bool> GetBookSummaryContents(BookSummaryCollection collection, int[] ids)
        {
            foreach (int id in ids)
            {
                var book = new BookSummary(id);
                await GetBookSummary(book);
                collection.Books.Add(book);
            }
            return true;
        }

        internal static async Task<bool> GetBookQuasiDetailContents(BookDetailCollection collection, int[] ids)
        {
            foreach (int id in ids)
            {
                var book = new BookDetail(id);
                await GetBookQuasiDetail(book);
                collection.Books.Add(book);
            }
            return true;
        }

        public static async Task<int[]> GetRelatedBooks(int BookId, int from = 0, int count = Util.RELATE_BOOK_AMOUNT)
        {
            var query = new QueryObject("GetRelatedBooks")
            {
                BookId = BookId,
                From = from,
                Count = count
            };
            var json = query.ToString();
            // TODO get book ids only 7
            await Task.Delay(100);
            List<int> ids = new List<int>();
            for (int i = from; i < from + count; ++i)
            {
                ids.Add(i + 564);
            }
            return ids.ToArray();
        }

        public static async void GetTitleDescription(BookDetailCollection collection, bool isBillboard, int id)
        {
            var query = new QueryObject("GetTitleDescription")
            {
                IsBillboard = isBillboard,
                BookListId = id
            };
            var json = query.ToString();
            // TODO
            await Task.Delay(delay);
            collection.Title = "Test billboard title";
            collection.CreateUser = "Test user name";
            collection.EditTime = DateTime.Now;
            collection.Description = "Test billboard descriptions: test test test test test test";
            collection.FollowAmount = 324;
            collection.OnPropertyChanged("Title");
            collection.OnPropertyChanged("CreateUser");
            collection.OnPropertyChanged("EditTime");
            collection.OnPropertyChanged("Description");
            collection.OnPropertyChanged("FollowAmount");

        }

        public static async Task<int[]> GetMyWishlist()
        {
            var query = new QueryObject("GetMyWishlist")
            {
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(100);
            List<int> ids = new List<int>();
            for (int i = 0; i < 11; ++i)
            {
                ids.Add(i + 789);
            }
            return ids.ToArray();
        }

        public static async Task<int[]> GetMyDanmus()
        {
            var query = new QueryObject("GetMyDanmus")
            {
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(100);
            List<int> ids = new List<int>();
            for (int i = 0; i < 35; ++i)
            {
                ids.Add(i + 123);
            }
            return ids.ToArray();
        }

        public static async Task<int[]> GetDanmuOfBook(int bookId, int page)
        {
            var query = new QueryObject("GetDanmuOfBook")
            {
                BookId = bookId,
                Page = page
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(100);
            List<int> ids = new List<int>();
            for (int i = 0; i < 20; ++i)
            {
                ids.Add(i + 123);
            }
            return ids.ToArray();
        }

        public static async Task<bool> GetDanmuContent(Danmu danmu)
        {
            var query = new QueryObject("GetDanmuContent")
            {
                DanmuId = danmu.ID
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(100);
            danmu.Content = new string[] { "6666666666666666666", "test test test test test" }[new Random().Next(2)];
            return true;
        }

        public static async Task<bool> GetFullDanmuContent(FullDanmu danmu)
        {
            var query = new QueryObject("GetFullDanmuContent")
            {
                DanmuId = danmu.ID
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(100);
            danmu.BookName = "Test book name";
            danmu.PageNum = 13;
            danmu.EditTime = DateTime.Now;
            danmu.Content = new string[] { "6666666666666666666", "test test test test test" }[new Random().Next(2)];
            danmu.UserName = "Myself";
            return true;
        }

        public static async Task<int[]> GetMyReadLists()
        {
            var query = new QueryObject("GetMyReadLists")
            {
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(100);
            List<int> ids = new List<int>();
            for (int i = 0; i < 3; ++i)
            {
                ids.Add(i + 456);
            }
            return ids.ToArray();
        }
    }

    public class NetworkSet
    {
        public static async Task<bool> Logout()
        {

            // TODO
            await Task.Delay(4000);
            Util.UserId = -1;
            Util.isAdmin = false;
            return true;
        }
    }

}
