package dao.impl;

import java.sql.SQLException;
import java.util.LinkedList;
import java.util.List;

import dao.BookDao;
import socket.BookhubServer;
import socket.InfoFromFront;
import socket.InfoToFront;

public class BookDaoImpl extends BaseDao implements BookDao {

	@Override
	public InfoToFront GetBookSummary(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();

		InfoToFront info = new InfoToFront();
		info.setType("GetBookSummary");
		getConnection();

		String sql = " select b.name as book_name, book_cover_url, a.name as author_name" + "from book b "
				+ "join book_author ba on b.id = ba.id " + "join author a on a.id = ba.id"
				+ "where b.id = ? and a.is_main_author is true ";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookId);
		rs = getPstmt().executeQuery();
		// if or while?
		if (rs.next()) {
			info.setBookCoverUrl(rs.getString("book_cover_url"));
			info.setBookName(rs.getString("book_name"));
			info.setAuthorName(rs.getString("author_name"));
		}

		closeAll();
		return info;
	}

	@Override
	public InfoToFront GetBookQuasiDetail(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();

		InfoToFront info = GetBookSummary(infoFromFront);

		getConnection();

		String sql = "select l.name as main, sl.name as sub, b.original_price, bs.discount, bs.overall_rating"
				+ "from book b" + "join book_stat bs on b.id = bs.book_id"
				+ "join sub_label sl on b.sublabel_id = sl.id" + "Ajoin label l on sl.main_id = l.id"
				+ "where b.id = ? ";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookId);
		rs = getPstmt().executeQuery();

		while (rs.next()) {
			info.setMainAndSubLabel(rs.getString("main") + "-" + rs.getString("sub"));
			info.setPrice(rs.getDouble("original_price"));
			info.setDisCount(rs.getInt("discount"));
			info.setOverallRating(rs.getDouble("overall_rating"));
		}

		closeAll();

		return info;
	}

	@Override
	public InfoToFront GetShelfBooks(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		List<Integer> shelf = new LinkedList<>();

		getConnection();

		String sql = "select book_id from transaction where user_id = ? and paied is true";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, userId);
		rs = getPstmt().executeQuery();

		while (rs.next()) {
			shelf.add(rs.getInt("book_id"));
		}

		InfoToFront info = new InfoToFront();
		info.setIDs(shelf);
		return info;
	}

	@Override
	public InfoToFront GetBookDetail(InfoFromFront infoFromFront) throws SQLException {
		InfoToFront info = GetBookQuasiDetail(infoFromFront);

		int bookId = infoFromFront.getBookId();
		int userId = infoFromFront.getUserId();

		String otherAuthorSql = null;
		otherAuthorSql = "select a.name, is_translator" + "from author a"
				+ "join book_author ba on a.id = ba.author_id" + "join book b on ba.book_id = b.id"
				+ "where is_main_author is false and b.id = ?";

		setPstmt(getConn().prepareStatement(otherAuthorSql));
		getPstmt().setInt(1, bookId);
		rs = getPstmt().executeQuery();

		StringBuilder stringBuilder = new StringBuilder();
		while (rs.next()) {
			Boolean isTranslator = rs.getBoolean("is_translator");
			String authorName = rs.getString("name");

			if (isTranslator)
				stringBuilder.append(authorName + "(Translator) ");
			else
				stringBuilder.append(authorName + " ");

		}

		info.setOtherAuthors(stringBuilder.toString().trim());

		String bookDetailSQL = "select b.description, b.publish_time, b.version, p.name,"
				+ "b.ISBN, b.pages, bs.buys, bs.danmus, bs.previews, bs.reviews"
				+ "from book b join book_stat bs on b.id = bs.book_id join press p on b.press_id = p.id"
				+ "where b.id = ?";

		setPstmt(getConn().prepareStatement(bookDetailSQL));
		getPstmt().setInt(1, bookId);
		rs = getPstmt().executeQuery();

		while (rs.next()) {
			info.setDescription(rs.getString("description"));
			info.setISBN(rs.getString("ISBN"));
			info.setBuyAmount(rs.getInt("buys"));
			info.setDanmuAmount(rs.getInt("danmus"));
			info.setPreviewAmount(rs.getInt("previews"));
			info.setReviewAmount(rs.getInt("reviews"));
			info.setPageCount(rs.getInt("pages"));

			String pressName = rs.getString("name");
			String publish_Time = rs.getDate("publish_time").toString();
			String version = rs.getString("version");

			info.setPublishInfo(pressName + " / " + publish_Time + " / " + version);

		}

		if (userId != -1) {
			String canAddReadlistSQL = "select book_id" + " from readlist r"
					+ " join readlist_books rb on r.id = rb.readlist_id"
					+ " where r.create_user = ? and rb.book_id = ?";

			setPstmt(getConn().prepareStatement(canAddReadlistSQL));
			getPstmt().setInt(1, userId);
			getPstmt().setInt(2, bookId);
			rs = getPstmt().executeQuery();

			if (rs.next() == false)
				info.setCanAddReadList(true);
			else
				info.setCanAddReadList(false);

			//
			String canAddWishlistSQL = "select w.user_id, w.book_id from wish_list w\n"
					+ "where user_id = ? and book_id = ?;";
			setPstmt(getConn().prepareStatement(canAddWishlistSQL));
			getPstmt().setInt(1, userId);
			getPstmt().setInt(2, bookId);
			rs = getPstmt().executeQuery();

			if (rs.next() == false)
				info.setCanAddWishList(true);
			else
				info.setCanAddWishList(false);

			//
			String canBuySQL = "select user_id, book_id, paied from transaction "
					+ "where user_id = ? and book_id = ? and paied is true;";

			setPstmt(getConn().prepareStatement(canBuySQL));
			getPstmt().setInt(1, userId);
			getPstmt().setInt(2, bookId);
			rs = getPstmt().executeQuery();

			if (rs.next() == false)
				info.setCanBuy(true);
			else
				info.setCanBuy(false);
		}
		return info;
	}

	@Override
	public InfoToFront GetRelatedBooks(InfoFromFront infoFromFront) throws SQLException {
		int bookid = infoFromFront.getBookId();
		int from = infoFromFront.getFrom();
		int count = infoFromFront.getCount();

		List<Integer> relatedbooks = new LinkedList<Integer>();
		getConnection();

		String sql = "select id from book" + " where label_id in (select label_id from book where id = ?)"
				+ " limit ? offset ?";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookid);
		getPstmt().setInt(2, count);
		getPstmt().setInt(3, from);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			relatedbooks.add(rs.getInt("id"));
		}

		closeAll();

		InfoToFront info = new InfoToFront();
		info.setIDs(relatedbooks);
		return info;
	}

	@Override
	public InfoToFront GetBookPreview(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();

		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "select preview_url from book where id = ?";

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookId);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			info.setURL(rs.getString("preview_url"));
		}

		closeAll();

		return info;
	}

	@Override
	public InfoToFront DownloadBook(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();

		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "select download_url from book where id = ?";

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookId);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			info.setURL(rs.getString("download_url"));
		}

		closeAll();

		return info;
	}

	@Override
	public InfoToFront GetBookKey(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();

		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "select pdf_password from book where id = ?";

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookId);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			info.setURL(rs.getString("pdf_password"));
		}

		closeAll();

		return info;
	}

	@Override
	public InfoToFront BuyBook(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		int bookId = infoFromFront.getBookId();

		String bookName = null;
		double price = 0;

		String sql = "select name, price_now(id) as price from book where book.id = ?";

		getConnection();

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookId);

		rs = getPstmt().executeQuery();
		while (rs.next()) {
			bookName = rs.getString("name");
			price = rs.getDouble("price");
		}
		closeAll();

		InfoToFront infoToFront = new InfoToFront();
		infoToFront.setURL(String.format("https://qr.alipay.com/?user=team309&name=%s&price=%f", bookName, price));
		BookhubServer.waitForPaying(userId, bookId);

		return infoToFront;
	}

	@Override
	public InfoToFront CheckBuyComplete(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		int bookId = infoFromFront.getBookId();
		InfoToFront infoToFront = new InfoToFront();

		getConnection();
		String sql = "select paid from transaction where transaction.user_id = ? and transaction.book_id = ?";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, userId);
		getPstmt().setInt(2, bookId);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			infoToFront.setSuccess(rs.getBoolean("paied"));
		}
		closeAll();
		return infoToFront;
	}

	@Override
	public InfoToFront CancelTransaction(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		int bookId = infoFromFront.getBookId();

		InfoToFront infoToFront = new InfoToFront();

		getConnection();

		boolean paid = false;
		String sqlQuery = "select paid from transaction where transaction.user_id = ? and transaction.book_id = ?";
		pstmt = conn.prepareStatement(sqlQuery);
		pstmt.setInt(1, userId);
		pstmt.setInt(2, bookId);

		rs = pstmt.executeQuery();
		while (rs.next()) {
			paid = rs.getBoolean("paid");
		}
		if (paid) {
			infoToFront.setSuccess(false);
			return infoToFront;
		}

		String sql = "delete from transaction where transaction.user_id = ? ,transaction.book_id = ?";
		pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, userId);
		pstmt.setInt(2, bookId);

		int rows = pstmt.executeUpdate();
		if (rows == 1) {
			infoToFront.setSuccess(true);
		}
		else {
			infoToFront.setSuccess(false);
		}
		closeAll();

		return infoToFront;
	}

}
