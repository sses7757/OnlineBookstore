﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frontend
{

    public class QueryObject
    {
        public string Type { set; get; }
        public string UserName { set; get; }
        public string EncodedPassword { set; get; }
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

        public int UserId { set; get; }

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

    public class Connection
    {
        public const string REMOTE_IP = "10.21.37.214";
        public const int REMOTE_PORT = 2307;

        private Socket socket;

        public Connection() { }

        internal async Task Reconnect()
        {
            if (socket != null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                finally { }
                socket.Close();
            }
            IPAddress ip = IPAddress.Parse(REMOTE_IP);
            IPEndPoint ipEnd = new IPEndPoint(ip, REMOTE_PORT);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Debug.WriteLine("Start connecting...");
                await socket.ConnectAsync(ipEnd);
            }
            catch (SocketException)
            {
                Debug.WriteLine("Fail to connect server, re-connecting...");
                await Task.Delay(Util.REFRESH_RATE);
                await this.Reconnect();
            }
        }

        private bool SocketConnected
        {
            get => !(socket == null ||
                     socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0);
        }

        /// <summary>
        /// Send to remote host with user id
        /// </summary>
        /// <param name="query"> A copied <see cref="QueryObject">QueryObject</see> </param>
        internal async Task SendWithUser(QueryObject query)
        {
            if (!this.SocketConnected)
            {
                await this.Reconnect();
            }
            query.UserId = Util.UserId;
            var send = Encoding.UTF8.GetBytes(query.ToJson());
            int sendLen;
            try
            {
                sendLen = await socket.SendAsync(send, SocketFlags.None);
                Debug.WriteLine("Send of {0} finish, total {1} bytes.", query.Type, sendLen);
            }
            catch (Exception)
            {
                await this.Reconnect();
                await this.SendWithUser(query);
            }
            
        }

        internal static async void Test()
        {
            var conn = new Connection();
            await conn.Reconnect();
            var socket = conn.socket;
            var stringSend = "{\n\"BookId\":123,\n\"bookName\":\"bookN\",\n\"authorId\":0,\n" +
                             "\"publisherId\":0,\n\"labelId\":0,\n\"pages\":0,\n\"price\":0.0\n}";
            //var stringSend = "{\"bookId\":123,\"bookName\":\"bookN\",\"authorId\":0," +
            //             "\"publisherId\":0,\"labelId\":0,\"pages\":0,\"price\":0.0}";
            var send = Encoding.UTF8.GetBytes(stringSend);
            int sendLen = await socket.SendAsync(send, SocketFlags.None);
            Debug.WriteLine("Send finish, total {0} bytes.", sendLen);

            socket.Shutdown(SocketShutdown.Send);

            byte[] receive = new byte[2048];
            int recvLen = await socket.ReceiveAsync(receive, SocketFlags.None);
            Debug.WriteLine("Test receive:\r\n" + Encoding.UTF8.GetString(receive, 0, recvLen));

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

    public class NetworkGet
    {
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
                EncodedPassword = EncodedPassword
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
            var newQuery = query.CloneThroughJson();
            newQuery.Type = "GetFromQuery";
            newQuery.From = from;
            newQuery.Count = count;
            var json = newQuery.ToJson();
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

        public static async Task<BookDetailCollection> GetTitleDescription(bool isBillboard, int id)
        {
            var query = new QueryObject("GetTitleDescription")
            {
                IsBillboard = isBillboard,
                BookListId = id
            };
            var json = query.ToString();
            // TODO
            await Task.Delay(delay);
            var collection = new BookDetailCollection();
            collection.Title = "Test billboard title";
            collection.Description = "Test billboard descriptions: test test test test test test";
            return collection;
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

        public static async Task<int[]> GetMyReadListsWithout(int bookId)
        {
            var query = new QueryObject("GetMyReadListsWithout")
            {
                BookId = bookId
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(1000);
            return new int[] { 323, 348, 978 };
        }

        public static async Task<string> GetBookPreview(int bookId)
        {
            var query = new QueryObject("GetBookPreview")
            {
                BookId = bookId
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(1000);
            return "file:///";
        }

        public static async Task<string> DownloadBook(int bookId)
        {
            var query = new QueryObject("DownloadBook")
            {
                BookId = bookId
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(1000);
            return "file:///";
        }

        public static async Task<string> GetBookKey(int bookId)
        {
            var query = new QueryObject("GetBookKey")
            {
                BookId = bookId
            };
            var json = query.ToString();
            // TODO 
            await Task.Delay(1000);
            return "asngaesgnaesiof";
        }
    }

    public class ChangeObject
    {
        public string Type { set; get; }
        public string UserName { set; get; }
        public string Email { set; get; }
        public string EncodedPassword { set; get; }

        public int? DanmuId { set; get; }
        public bool? IsDeleteAction { set; get; }
        public string NewContent { set; get; }

        public int? ReadListId { set; get; }
        public ReadListChangeType? ChangeType { set; get; }
        public int? AlteredBookId { set; get; }
        public string AlteredText { set; get; }

        public int? BookId { set; get; }
        public bool? IsAddAction { set; get; }

        public string Content { set; get; }
        public int? PageNum { set; get; }

        public string Title { set; get; }
        
        public int? Rating { set; get; }


        public ChangeObject() { }

        public ChangeObject(string type)
        {
            Type = type;
        }

        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(this, settings);
        }
    }

    public class NetworkSet
    {
        public static async Task<bool> Logout()
        {
            var change = new ChangeObject("Logout")
            {
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(2000);
            Util.UserId = -1;
            Util.isAdmin = false;
            return true;
        }

        public static async Task<bool> SignUp(string userName, string email, string password)
        {
            var change = new ChangeObject("SignUp")
            {
                UserName = userName,
                Email = email,
                EncodedPassword = password
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(2000);
            return true;
        }

        public static async Task<bool> ChangeDanmu(int danmuId, bool isDeleteAction,
                                                   string newContent = null)
        {
            var change = new ChangeObject("ChangeDanmu")
            {
                DanmuId = danmuId,
                IsDeleteAction = isDeleteAction,
                NewContent = newContent
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(1000);
            return true;
        }

        public static async Task<bool> ChangeReadList(int readListId, ReadListChangeType changeType,
                                                      int? alteredId = null, string alteredText = null)
        {
            var change = new ChangeObject("ChangeReadList")
            {
                ReadListId = readListId,
                ChangeType = changeType,
                AlteredBookId = alteredId,
                AlteredText = alteredText
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(1000);
            return true;
        }

        public static async Task<bool> ChangeWishlist(int bookId, bool isAddAction)
        {
            var change = new ChangeObject("ChangeWishlist")
            {
                BookId = bookId,
                IsAddAction = isAddAction
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(1000);
            return true;
        }

        public static async Task<bool> CreateDanmu(string content, int bookId, int pageNum)
        {
            var change = new ChangeObject("CreateDanmu")
            {
                Content = content,
                BookId = bookId,
                PageNum = pageNum
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(1000);
            return true;
        }

        public static async Task<bool> CreateReadList(string title, string content)
        {
            var change = new ChangeObject("CreateReadList")
            {
                Content = content,
                Title = title
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(1000);
            return true;
        }

        public static async Task<string> BuyBook(int bookId)
        {
            var change = new ChangeObject("BuyBook")
            {
                BookId = bookId
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(1000);
            return "http://test-url/buy?bookId=" + bookId;
        }

        public static async Task<bool> CreateReview(int bookId, int rating, string title, string content)
        {
            var change = new ChangeObject("CreateReview")
            {
                BookId = bookId,
                Rating = rating,
                Title = title,
                Content = content
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(1000);
            return true;
        }

        public static async Task<bool> CheckBuyComplete(int bookId)
        {
            var change = new ChangeObject("CheckBuyComplete")
            {
                BookId = bookId,
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(100);
            return true;
        }

        public static async Task<bool> CancleTransaction(int bookId)
        {
            var change = new ChangeObject("CancleTransaction")
            {
                BookId = bookId,
            };
            var json = change.ToJson();
            // TODO
            await Task.Delay(100);
            return true;
        }
    }
}
