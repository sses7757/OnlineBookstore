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

		String sql = "select review_id" + " from review" + " where book_id = ?" + " order by edit_time"
				+ " limit ? offset ?";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookId);
		getPstmt().setInt(2, count);
		getPstmt().setInt(3, from);
		rs = getPstmt().executeQuery();

		while (rs.next()) {
			reviewId.add(rs.getInt("review_id"));
		}

		InfoToFront info = new InfoToFront();
		info.setIDs(reviewId);
		return info;
	}

	@Override
	public InfoToFront GetReview(InfoFromFront infoFromFront) throws SQLException {
		int reviewId = infoFromFront.getReviewId();
		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "select r.user_id, r.rating, r.edit_time, r.title, r.content, b.name as book_name"
				+ " from review r" + " join book b on r.book_id = b.id" + " where r.review_id = ?";

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, reviewId);
		rs = getPstmt().executeQuery();

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

		String sql = "";
		if (isDeleteAction) {
			sql += "DELETE FROM review WHERE user_id = ? AND review_id = ?";
			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setInt(1, userId);
			getPstmt().setInt(2, reviewId);
		}
		else {
			sql += "UPDATE review " + "SET title = ? , content = ? , rating = ?"
					+ "WHERE review_id = ? AND user_id = ?";

			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setString(1, newTitle);
			getPstmt().setString(2, newContent);
			getPstmt().setInt(3, newRating);

		}
		int rows = getPstmt().executeUpdate();

		if (rows == 1)
			info.setSuccess(true);
		else
			info.setSuccess(false);

		return info;
	}

	@Override
	public InfoToFront CreateReview(InfoFromFront infoFromFront) throws SQLException {
		int bookId, userId, rating;
		String title, content;
		bookId = infoFromFront.getBookId();
		userId = infoFromFront.getUserId();
		rating = infoFromFront.getRating();
		title = infoFromFront.getTitle();
		content = infoFromFront.getContent();

		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "insert into review(user_id, book_id, title, content, rating) values(?,?,?,?,?)";

		pstmt.setInt(1, userId);
		pstmt.setInt(2, bookId);
		pstmt.setString(3, title);
		pstmt.setString(4, content);
		pstmt.setInt(5, rating);

		pstmt = conn.prepareStatement(sql);

		int rows = pstmt.executeUpdate();
		info.setSuccess(rows == 1);

		closeAll();
		return info;
	}
}
