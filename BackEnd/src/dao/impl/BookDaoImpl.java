package dao.impl;

import java.sql.SQLException;
import java.util.LinkedList;
import java.util.List;

import dao.BookDao;
import socket.InfoFromFront;
import socket.InfoToFront;

public class BookDaoImpl extends BaseDao implements BookDao {

	@Override
	public InfoToFront GetBookSummary(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();

		InfoToFront dataToFront = new InfoToFront();
		dataToFront.setType("GetBookSummary");
		getConnection();

		String sql = " select b.name as book_name, book_cover_url, a.name as author_name" + "from book b "
				+ "join book_author ba on b.id = ba.id " + "join author a on a.id = ba.id"
				+ "where b.id = ? and a.is_main_author is true ";
		pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, bookId);
		rs = pstmt.executeQuery();
		// if or while?
		if (rs.next()) {
			dataToFront.setBookCoverUrl(rs.getString("book_cover_url"));
			dataToFront.setBookName(rs.getString("book_name"));
			dataToFront.setAuthorName(rs.getString("author_name"));
		}

		closeAll();
		return dataToFront;
	}

	@Override
	public InfoToFront GetBookQuasiDetail(InfoFromFront infoFromFront) throws SQLException {
		InfoToFront dataToFront = GetBookSummary(infoFromFront);
		int bookId = infoFromFront.getBookId();
		dataToFront.setType("GetBookQuasiDetail");
		try {
			getConnection();

			String sql = "select l.name as main, sl.name as sub, b.original_price, bs.discount, bs.overall_rating"
					+ "from book b" + "join book_stat bs on b.id = bs.book_id"
					+ "join sub_label sl on b.sublabel_id = sl.id" + "Ajoin label l on sl.main_id = l.id"
					+ "where b.id = ? ";
			pstmt = conn.prepareStatement(sql);
			pstmt.setInt(1, bookId);
			rs = pstmt.executeQuery();

			while (rs.next()) {
				dataToFront.setMainAndSubLabel(rs.getString("main") + "-" + rs.getString("sub"));
				dataToFront.setPrice(rs.getDouble("original_price"));
				dataToFront.setDisCount(rs.getInt("discount"));
				dataToFront.setOverallRating(rs.getDouble("overall_rating"));
			}

			closeAll();
		} catch (SQLException e) {
			e.printStackTrace();
		}
		return dataToFront;
	}

	@Override
	public InfoToFront GetShelfBooks(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		List<Integer> shelf = new LinkedList<>();

		getConnection();

		String sql = "select book_id from transaction where user_id = ? and paied is true";
		pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, userId);
		rs = pstmt.executeQuery();

		while (rs.next()) {
			shelf.add(rs.getInt("book_id"));
		}

		InfoToFront info = new InfoToFront();
		info.setIDs(shelf);
		return null;
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

		pstmt = conn.prepareStatement(otherAuthorSql);
		pstmt.setInt(1, bookId);
		rs = pstmt.executeQuery();

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

		pstmt = conn.prepareStatement(bookDetailSQL);
		pstmt.setInt(1, bookId);
		rs = pstmt.executeQuery();

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
			String canAddReadlistSQL = "select book_id\n" + "from readlist r\n"
					+ "    join readlist_books rb on r.id = rb.readlist_id\n"
					+ "where r.create_user = ? and rb.book_id = ?";

			pstmt = conn.prepareStatement(canAddReadlistSQL);
			pstmt.setInt(1, userId);
			pstmt.setInt(2, bookId);
			rs = pstmt.executeQuery();

			if (rs.next() == false)
				info.setCanAddReadList(true);
			else
				info.setCanAddReadList(false);

			//
			String canAddWishlistSQL = "select w.user_id, w.book_id from wish_list w\n"
					+ "where user_id = ? and book_id = ?;";
			pstmt = conn.prepareStatement(canAddWishlistSQL);
			pstmt.setInt(1, userId);
			pstmt.setInt(2, bookId);
			rs = pstmt.executeQuery();

			if (rs.next() == false)
				info.setCanAddWishList(true);
			else
				info.setCanAddWishList(false);

			//
			String canBuySQL = "select user_id, book_id, paied from transaction "
					+ "where user_id = ? and book_id = ? and paied is true;";

			pstmt = conn.prepareStatement(canBuySQL);
			pstmt.setInt(1, userId);
			pstmt.setInt(2, bookId);
			rs = pstmt.executeQuery();

			if (rs.next() == false)
				info.setCanBuy(true);
			else
				info.setCanBuy(false);
		}
		return info;
	}

	@Override
	public InfoToFront GetRelatedBooks(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();
		int from = infoFromFront.getFrom();
		int count = infoFromFront.getCount();
		return null;
	}

	@Override
	public InfoToFront GetBookPreview(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();
		return null;
	}

	@Override
	public InfoToFront DownloadBook(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();
		return null;
	}

	@Override
	public InfoToFront GetBookKey(InfoFromFront infoFromFront) throws SQLException {
		int bookId = infoFromFront.getBookId();
		return null;
	}

	@Override
	public InfoToFront BuyBook(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		int bookId = infoFromFront.getBookId();
		return null;
	}

	@Override
	public InfoToFront CheckBuyComplete(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		int bookId = infoFromFront.getBookId();
		return null;
	}

	@Override
	public InfoToFront CancelTransaction(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		int bookId = infoFromFront.getBookId();
		return null;
	}

}
