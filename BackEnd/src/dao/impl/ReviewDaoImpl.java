package dao.impl;

import java.sql.SQLException;
import java.util.LinkedList;
import java.util.List;

import dao.ReviewDao;
import socket.InfoFromFront;
import socket.InfoToFront;

public class ReviewDaoImpl extends BaseDao implements ReviewDao {

	@Override
	public InfoToFront GetBookReviews(InfoFromFront infoFromFront) throws SQLException {
		int bookId, from, count;
		bookId = infoFromFront.getBookId();
		from = infoFromFront.getFrom();
		count = infoFromFront.getCount();
		List<Integer> reviewId = new LinkedList<Integer>();
		getConnection();

		String sql = " select review_id from review order by edit_time" + "where book_id = ?";
		pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, bookId);
		rs = pstmt.executeQuery();

		int i = 0, j = 0;
		while (rs.next() && i < from) {
			i++;
		}
		while (rs.next() && j < count) {
			reviewId.add(rs.getInt("review_id"));
		}

		InfoToFront info = new InfoToFront();
		info.setIDs(reviewId);
		return info;
	}

	@Override
	public InfoToFront GetReviews(InfoFromFront infoFromFront) throws SQLException {
		int reviewId = infoFromFront.getReviewId();
		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "select r.user_id, r.rating, r.edit_time, r.title, r.content, b.name as book_name"
				+ "from review r join book b on r.book_id = b.id" + "where r.review_id = ?";

		pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, reviewId);
		rs = pstmt.executeQuery();

		while (rs.next()) {
			info.setCreateUser(rs.getString("user_id"));
			info.setRating(rs.getInt("rating"));
			info.setTimeStap(rs.getTimestamp("edit_time").getTime());
			info.setTitle(rs.getString("title"));
			info.setContent(rs.getString("content"));
			info.setBookName(rs.getString("book_name"));
		}

		return info;
	}

	@Override
	public InfoToFront ChangeReview(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		int reviewId = infoFromFront.getReviewId();
		boolean isDeleteAction = infoFromFront.getDeleteAction();
		String newTitle = infoFromFront.getTitle();
		String newContent = infoFromFront.getNewContent();
		int newRating = infoFromFront.getRating();

		InfoToFront info = new InfoToFront();
		info.setType("ChangeReview");

		String sql = null;
		if (isDeleteAction) {
			sql = "DELETE FROM review WHERE user_id = ? AND review_id = ?";
			pstmt = conn.prepareStatement(sql);
			pstmt.setInt(1, userId);
			pstmt.setInt(2, reviewId);
		}
		else {
			sql = "set sql_safe_updates = 1" + "UPDATE review " + "SET title = ? , content = ? , rating = ?"
					+ "WHERE review_id = ? AND user_id = ?";

			pstmt = conn.prepareStatement(sql);
			pstmt.setString(1, newTitle);
			pstmt.setString(2, newContent);
			pstmt.setInt(3, newRating);

		}
		int rows = pstmt.executeUpdate();

		if (rows == 1)
			info.setSuccess(true);
		else
			info.setSuccess(false);

		return info;
	}

	@Override
	public InfoToFront CreateReview(InfoFromFront infoFromFront) throws SQLException {
		int bookId, rating;
		String title, content;
		bookId = infoFromFront.getBookId();
		rating = infoFromFront.getRating();
		title = infoFromFront.getTitle();
		content = infoFromFront.getContent();
		return null;
	}

	@Override
	public InfoToFront CheckBuyComplete(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		int bookId = infoFromFront.getBookId();

		return null;
	}

}
