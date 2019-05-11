﻿using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Frontend
{
	public enum BookListChangeType
	{
		AddBook,
		RemoveList,
		DeleteBook,
		ChangeTitle,
		ChangeDescription
	}



	public class DanmuCollection : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		internal void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		internal int BookId { private set; get; }
		internal uint PageNum { private set; get; }
		internal List<Danmu> Danmus { private set; get; } = new List<Danmu>();
		internal bool Finish { private set; get; } = false;

		public DanmuCollection(int bookId, uint page)
		{
			this.BookId = bookId;
			this.PageNum = page;
		}

		internal async Task Reload()
		{
			this.Danmus.Clear();
			this.Finish = false;
			// Read global settings to find how many danmus
			var ids = await NetworkGet.GetDanmuOfBook(this.BookId, this.PageNum);
			foreach (int id in ids)
			{
				var dan = new Danmu(id);
				await NetworkGet.GetDanmuContent(dan);
				this.Danmus.Add(dan);
			}
			this.Finish = true;
		}
	}

	public class FullDanmu : INotifyPropertyChanged
	{
		internal DateTime EditTime { set; get; }
		internal string BookName { set; get; }
		internal int PageNum { set; get; }
		internal string Content { set; get; }
		internal int ID { set; get; }

		public FullDanmu(int id)
		{
			this.ID = id;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		internal void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	public class Danmu
	{
		internal string Content { set; get; }
		internal int ID { set; get; }

		public Danmu(int id)
		{
			this.ID = id;
		}
	}


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

	internal class SearchInfo : INotifyPropertyChanged
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
			if (index < 0)
				return;
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

		internal BookDetailCollection Books { set; get; }
		internal BooklistCollection Billboards { set; get; }
		internal BooklistCollection ReadLists { set; get; }

		private void Init()
		{
			foreach (BooksOrderType t in Enum.GetValues(typeof(BooksOrderType)))
				BookOrders.Add(new ComboBoxItem { Content = t.EnumToString()});
			foreach (BillboardsOrderType t in Enum.GetValues(typeof(BillboardsOrderType)))
				BillboardOrders.Add(new ComboBoxItem { Content = t.EnumToString() });
			foreach (ReadlistsOrderType t in Enum.GetValues(typeof(ReadlistsOrderType)))
				ReadListOrders.Add(new ComboBoxItem { Content = t.EnumToString() });
			Books = new BookDetailCollection(this.ToQueryObject(ContentType.Books),
											 "Search result of " + this.QueryText, "");
			Billboards = new BooklistCollection(true, this.ToQueryObject(ContentType.Billboards));
			ReadLists = new BooklistCollection(false, this.ToQueryObject(ContentType.ReadLists));
		}

		internal void Refresh(bool add = false, bool force = false)
		{
			if (!force)
			{
				switch (this.QueryType)
				{
					case ContentType.Books:
						if (!Books.Finished)
							return;
						break;
					case ContentType.Billboards:
						if (!Billboards.Finished)
							return;
						break;
					case ContentType.ReadLists:
						if (!ReadLists.Finished)
							return;
						break;
					default:
						return;
				}
			}

			if (add)
			{
				switch (this.QueryType)
				{
					case ContentType.Books:
						Books.AddBooks();
						break;
					case ContentType.Billboards:
						Billboards.Reload(true);
						break;
					case ContentType.ReadLists:
						ReadLists.Reload(true);
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
						_ = Books.ReloadBooks(this.ToQueryObject(), "Search result of " + this.QueryText);
						break;
					case ContentType.Billboards:
						Billboards.Reload();
						break;
					case ContentType.ReadLists:
						ReadLists.Reload();
						break;
					default:
						return;
				}
			}
		}

		private QueryObject ToQueryObject()
		{
			return this.ToQueryObject(this.QueryType);
		}

		private QueryObject ToQueryObject(ContentType type)
		{
			QueryObject query = new QueryObject()
			{
				SearchType = (int)type,
				IsBillboard = type == ContentType.Books ? (bool?)null :
								(type == ContentType.Billboards ? true : false),
				QueryText = QueryText,
				Order = this.OrderToIndex(type),
				OrderDescend = OrderDescend,
				TimeRangeType = (int)TimeRangeType,
				TimeRange = TimeRange.ToArray(),
				PageRange = PageRange.ToArray(),
				IncludeFreeBooks = IncludeFreeBooks,
				LabelFilters = LabelFilters.ToArray()
			};

			return query;
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
	}

	public class Label : INotifyPropertyChanged
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
				NetworkGet.GetSubLabels(this);
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

	public class SubLabel : INotifyPropertyChanged
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

	public class Review
	{
		internal readonly int ID = -1;
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

		internal Review(int id)
		{
			this.ID = id;
		}
	}

	public class BookDetail : BookSummary
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
			RelatedBooks = new BookSummaryCollection(BookSummaryCollection.OtherType.RelatedBooks, ID);
			_ = NetworkGet.GetBookDetail(this);
		}

		internal BookDetail(int id) : base(id) { }

		internal void GetMoreReview()
		{
			finished = false;
			_ = NetworkGet.GetReviewContents(this, true, this.Reviews.Count);
		}
	}

	/// <summary>
	/// Collection for showing billboards / read lists
	/// </summary>
	public class BooklistCollection: INotifyPropertyChanged
	{
		internal ObservableCollection<BookDetailCollection> Booklists { set; get; }
			= new ObservableCollection<BookDetailCollection>();

		private const int INIT_AMOUNT = 4;
		private const int ADD_AMOUNT = 2;

		public event PropertyChangedEventHandler PropertyChanged;

		internal void OnPropertyChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Booklists"));
		}

		private readonly bool isBillBoard;
		private readonly QueryObject query;

		internal BooklistCollection(bool isBillBoard)
		{
			this.isBillBoard = isBillBoard;
			this.query = null;
			Reload();
		}

		internal BooklistCollection(bool isBillBoard, QueryObject query)
		{
			this.isBillBoard = isBillBoard;
			this.query = query;
		}

		internal bool Finished { get; private set; } = false;

		internal async void Reload(bool addMore = false)
		{
			this.Finished = false;
			if (!addMore)
			{
				Booklists.Clear();
			}

			int[] ids;
			if (addMore)
			{
				if (this.query == null)
				{
					if (this.isBillBoard)
						ids = await NetworkGet.GetFromQuery(NetworkGet.BillboardTop, Booklists.Count, ADD_AMOUNT);
					else
						ids = await NetworkGet.GetFromQuery(NetworkGet.ReadListTop, Booklists.Count, ADD_AMOUNT);
				}
				else
				{
					ids = await NetworkGet.GetFromQuery(this.query, Booklists.Count, ADD_AMOUNT);
				}
			}
			else
			{
				if (this.query == null)
				{
					if (this.isBillBoard)
						ids = await NetworkGet.GetFromQuery(NetworkGet.BillboardTop, 0, INIT_AMOUNT);
					else
						ids = await NetworkGet.GetFromQuery(NetworkGet.ReadListTop, 0, INIT_AMOUNT);
				}
				else
				{
					ids = await NetworkGet.GetFromQuery(query, 0, INIT_AMOUNT);
				}
			}
			foreach (int id in ids)
			{
				var collection = new BookDetailCollection(this.isBillBoard, id);
				await collection.ReloadBooks(this.isBillBoard, id);
				Booklists.Add(collection);
			}
			this.Finished = true;
		}
	}

	/// <summary>
	/// Collection for showing book summary with more infos
	/// </summary>
	public class BookDetailCollection : INotifyPropertyChanged
	{
		internal ObservableCollection<BookDetail> Books { set; get; } = new ObservableCollection<BookDetail>();
		internal string Title { set; get; }
		internal string Description { set; get; }
		internal string CreateUser { set; get; }
		internal DateTime EditTime { set; get; }
		internal int FollowAmount { set; get; }
		internal bool Finished { get; private set; } = false;

		internal int? ID {
			get => this.query.BookListId;
		}

		internal QueryObject query;

		public event PropertyChangedEventHandler PropertyChanged;

		internal void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		internal BookDetailCollection(QueryObject query, string title, string content)
		{
			if (query.SearchType == null)
			{
				this.query = query;
				Title = title;
				Description = content;
				_ = this.ReloadBooks(query.IsBillboard.Value, query.BookListId.Value, Util.INIT_AMOUNT);
				return;
			}
			if (query.SearchType != (int)ContentType.Books)
			{
				Console.Error.WriteLine("wrong query search type");
				return;
			}
			this.query = query;
			Title = title;
			Description = content;
			_ = this.ReloadBooks();
		}

		internal BookDetailCollection(bool isBillboard, int id)
		{
			this.query = new QueryObject()
			{
				IsBillboard = isBillboard,
				BookListId = id
			};
			NetworkGet.GetTitleDescription(this, isBillboard, id);
			//_ = this.ReloadBooks(isBillboard, id);
		}

		public BookDetailCollection()
		{
		}

		internal async Task<bool> ReloadBooks(bool isBillboard, int id, int count = Util.ADD_AMOUNT)
		{
			this.Books.Clear();
			this.Finished = false;
			var ids = await NetworkGet.GetBookListBooks(isBillboard, id, 0, count);
			await NetworkGet.GetBookQuasiDetailContents(this, ids);
			this.Finished = true;
			return true;
		}

		internal async Task<bool> ReloadBooks()
		{
			return await this.ReloadBooks(this.query);
		}

		internal async Task<bool> ReloadBooks(QueryObject newQuery, string newTitle = null, string newDesc = null)
		{
			if (newQuery.SearchType == null)
			{
				return await this.ReloadBooks(query.IsBillboard.Value, query.BookListId.Value);
			}
			this.Books.Clear();
			this.query = newQuery;
			if (newTitle != null)
				this.Title = newTitle;
			if (newDesc != null)
				this.Description = newDesc;
			this.OnPropertyChanged("Title");
			this.Finished = false;
			var ids = await NetworkGet.GetFromQuery(this.query, 0, Util.INIT_AMOUNT);
			await NetworkGet.GetBookQuasiDetailContents(this, ids);
			this.Finished = true;
			return true;
		}

		private async Task AddBooks(bool isBillboard, int id)
		{
			this.Finished = false;
			var ids = await NetworkGet.GetBookListBooks(isBillboard, id, this.Books.Count, Util.ADD_AMOUNT);
			await NetworkGet.GetBookQuasiDetailContents(this, ids);
			this.Finished = true;
		}

		internal async void AddBooks()
		{
			if (this.query.SearchType == null)
			{
				await this.AddBooks(query.IsBillboard.Value, query.BookListId.Value);
				return;
			}
			this.Finished = false;
			var ids = await NetworkGet.GetFromQuery(this.query, this.Books.Count, Util.ADD_AMOUNT);
			await NetworkGet.GetBookQuasiDetailContents(this, ids);
			this.Finished = true;
		}
	}

	internal enum BookSummaryCollectionType
	{
		PersonalRecommands,
		TopBooks,
		NewBooks,
		Other
	}

	/// <summary>
	/// Collection for showing book summary
	/// </summary>
	internal class BookSummaryCollection : INotifyPropertyChanged
	{
		internal enum OtherType
		{
			Bookshelf,
			RelatedBooks
		}

		internal ObservableCollection<BookSummary> Books { set; get; } = new ObservableCollection<BookSummary>();

		internal bool Finished { get; private set; } = false;

		public event PropertyChangedEventHandler PropertyChanged;

		internal void OnPropertyChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Books"));
		}

		internal async void AddBooks()
		{
			if (this.type.Item2.Value != OtherType.RelatedBooks ||
				!NetworkGet.IsValidID(this.type.relateBookId.Value))
			{
				return;
			}
			this.Finished = false;
			var ids = await NetworkGet.GetRelatedBooks(this.type.relateBookId.Value,
													   this.Books.Count, Util.ADD_AMOUNT);
			await NetworkGet.GetBookSummaryContents(this, ids);
			this.Finished = true;
		}

		internal async Task Reload()
		{
			this.Books.Clear();
			this.Finished = false;
			int[] ids;
			if (this.type.Item1.HasValue)
			{
				switch (this.type.Item1.Value)
				{
					case BookSummaryCollectionType.PersonalRecommands:
						ids = await NetworkGet.GetFromQuery(NetworkGet.PersonalRecommend, 0, Util.PREVIEW_AMOUNT);
						break;
					case BookSummaryCollectionType.TopBooks:
						ids = await NetworkGet.GetFromQuery(NetworkGet.TopBooks, 0, Util.PREVIEW_AMOUNT);
						break;
					case BookSummaryCollectionType.NewBooks:
						ids = await NetworkGet.GetFromQuery(NetworkGet.NewBooks, 0, Util.PREVIEW_AMOUNT);
						break;
					default:
						this.Finished = true;
						return;
				}
			}
			else
			{
				switch (this.type.Item2.Value)
				{
					case OtherType.Bookshelf:
						ids = await NetworkGet.GetShelfBooks();
						break;
					case OtherType.RelatedBooks:
						if (!NetworkGet.IsValidID(this.type.relateBookId.Value))
						{
							this.Finished = true;
							return;
						}
						ids = await NetworkGet.GetRelatedBooks(this.type.relateBookId.Value,
															   0, Util.RELATE_BOOK_AMOUNT);
							break;
					default:
						this.Finished = true;
						return;
				}
			}
			await NetworkGet.GetBookSummaryContents(this, ids);
			this.Finished = true;
		}

		/// <summary>
		/// ValueTuple
		/// </summary>
		private (BookSummaryCollectionType?, OtherType?, int? relateBookId) type;

		internal BookSummaryCollection(BookSummaryCollectionType type)
		{
			this.type.Item1 = type;
			_ = this.Reload();
		}

		internal BookSummaryCollection(OtherType type, int relateBookId = -1)
		{
			this.type.Item2 = type;
			this.type.relateBookId = relateBookId;
		}
	}

	public class BookSummary
	{
		internal int ID { set; get; }
		internal string BookName { set; get; }
		internal string BookFullName { set; get; }
		internal BitmapImage BookCover { set; get; }
		internal string Author { set; get; }

		internal BookSummary(BookSummary book)
		{
			this.BookCover = book.BookCover;
			this.ID = book.ID;
			this.BookName = book.BookName;
			this.BookFullName = book.BookFullName;
			this.Author = book.Author;
		}

		internal BookSummary(int BookId)
		{
			this.ID = BookId;
		}
	}

	internal static class Util
	{
		internal const int PREVIEW_AMOUNT = 8;
		internal const int INIT_AMOUNT = 14;
		internal const int ADD_AMOUNT = 6;
		internal const int RELATE_BOOK_AMOUNT = 7;
		internal const int REVIEW_AMOUNT_ONE_TIME = 4;

		internal const string TO_BOOK_DETAIL = "toDetail";
		internal const string FROM_BOOK_DETAIL = "fromDetail";

		internal const int REFRESH_RATE = 500;

		internal const string WAIT_STR = "Waiting...";

		internal static MainPage main;
		internal static int UserId { set; get; } = -1;
		internal static bool IsAdmin { set; get; } = false;
		internal static Color DanmuColor { set; get; } = Colors.Black;
		internal static float DanmuSize { set; get; } = 20;
		internal static float DanmuSpacing { set; get; } = 6;
		internal static bool IsDanmuOn { set; get; } = true;
		internal static Visibility DanmuVisibility { get => IsDanmuOn.ToVisibility(); }
		internal static CanvasTextFormat DanmuTextFormat { get; }
			= new CanvasTextFormat
			{
				HorizontalAlignment = CanvasHorizontalAlignment.Center,
				VerticalAlignment = CanvasVerticalAlignment.Center,
				Options = CanvasDrawTextOptions.NoPixelSnap
			};

	internal static ObservableCollection<Label> LABELS;

		internal static Visibility ToVisibility(this bool visible)
		{
			return visible ? Visibility.Visible : Visibility.Collapsed;
		}

		internal static string SHA256(string data)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			byte[] hash = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);

			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				builder.Append(hash[i].ToString("X2"));
			}

			return builder.ToString();
		}

		internal const int LEVEL_DataTemplate = 10;

		internal static UIElement GetParentUpto(this UIElement elem, int level = 1)
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

		internal static string EnumToString(this Enum e)
		{
			return System.Text.RegularExpressions.Regex.Replace(e.ToString("F"), @"(\p{Lu})", " $1").TrimStart();
		}

		internal static T CloneThroughJson<T>(this T source)
		{
			if (source == null)
			{
				return default;
			}
			var deserializeSettings = new JsonSerializerSettings
				{ ObjectCreationHandling = ObjectCreationHandling.Replace };
			return JsonConvert.DeserializeObject<T>
				(JsonConvert.SerializeObject(source), deserializeSettings);
		}

		internal static async Task<string> InputTextDialogAsync(string title, string placeholder,
																string previousContent)
		{
			TextBox inputTextBox = new TextBox
			{
				AcceptsReturn = false,
				TextWrapping = TextWrapping.Wrap,
				MinHeight = 32,
				MaxWidth = 650,
				Text = previousContent,
				PlaceholderText = placeholder
			};
			ContentDialog dialog = new ContentDialog
			{
				//HorizontalAlignment = HorizontalAlignment.Center,
				//VerticalAlignment = VerticalAlignment.Center,
				Content = inputTextBox,
				Title = title,
				IsSecondaryButtonEnabled = true,
				PrimaryButtonText = "Confirm",
				SecondaryButtonText = "Cancel"
			};
			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
				return inputTextBox.Text;
			else
				return previousContent;
		}

		public static async Task<BitmapImage> ToQRCode(this string str)
		{
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(str, QRCodeGenerator.ECCLevel.Q);

			//Create byte/raw bitmap qr code
			BitmapByteQRCode qrCodeBmp = new BitmapByteQRCode(qrCodeData);
			byte[] qrCodeImageBmp = qrCodeBmp.GetGraphic(20);
			InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
			DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0));
			writer.WriteBytes(qrCodeImageBmp);
			await writer.StoreAsync();
			var image = new BitmapImage();
			await image.SetSourceAsync(stream);
			return image;
		}

		public static bool IsWebUri(this Uri uri)
		{
			if (uri != null)
			{
				var str = uri.ToString().ToLower();
				return str.StartsWith("http://") || str.StartsWith("https://");
			}
			return false;
		}

		private static object lockObj = null;
		private static readonly Random random = new Random();

		/// <summary>
		/// Global lock of any method
		/// </summary>
		/// <typeparam name="TIn">input type</typeparam>
		/// <typeparam name="TOut">output type</typeparam>
		/// <param name="func">an async method that actually dose computing</param>
		/// <param name="parameter">the parameter need to pass to the async method</param>
		/// <returns></returns>
		public static async Task<TOut> GlobalLock<TIn, TOut>(this Func<TIn, Task<TOut>> func, TIn parameter) //where
		{
			await Task.Delay(random.Next(REFRESH_RATE / 5, REFRESH_RATE / 3));
			while (lockObj != null)
			{
				await Task.Delay(REFRESH_RATE / 2);
			}
			System.Diagnostics.Debug.WriteLine("GL start:\t" + DateTime.Now.Second +
												":" + DateTime.Now.Millisecond);
			lockObj = new object();
			var result = await func(parameter);
			lockObj = null;
			System.Diagnostics.Debug.WriteLine("GL end:\t" + DateTime.Now.Second +
												":" + DateTime.Now.Millisecond);
			return result;
		}

		/// <summary>
		/// time stap to date time
		/// </summary>
		/// <param name="TimeStamp">time stamp</param>
		/// <returns>DateTime</returns>
		public static DateTime GetTime(this long TimeStamp)
		{
			DateTime startTime = new DateTime(1970, 1, 1); // 当地时区
			return startTime.AddSeconds(TimeStamp);
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

		internal T[] ToArray()
		{
			if (this.Maximum.CompareTo(this.inf) == 0 && this.inf.CompareTo(default) > 0)
				return new T[] { this.Minimum, (T)typeof(T).GetField("MaxValue").GetValue(null)};
			else if (this.Minimum.CompareTo(this.minusInf) == 0 && this.minusInf.CompareTo(default) < 0)
				return new T[] { (T)typeof(T).GetField("MinValue").GetValue(null), this.Maximum };
			else
				return new T[] { this.Minimum, this.Maximum };
		}


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
