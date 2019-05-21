package dao.impl;

import java.sql.SQLException;
import java.util.LinkedList;
import java.util.List;

import dao.WishlistDao;
import socket.InfoFromFront;
import socket.InfoToFront;

public class WishlistDaoImpl extends BaseDao implements WishlistDao {
	@Override
	public InfoToFront GetMyWishlist(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		List<Integer> wishlist = new LinkedList<Integer>();

		getConnection();

		String sql = "select book_id from wish_list where user_id = ?";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, userId);
		rs = getPstmt().executeQuery();

		while (rs.next()) {
			wishlist.add(rs.getInt("book_id"));
		}

		closeAll();

		InfoToFront info = new InfoToFront();
		info.setIDs(wishlist);
		return info;
	}

	@Override
	public InfoToFront ChangeWishlist(InfoFromFront infoFromFront) throws SQLException {
		int userid = infoFromFront.getUserId();
		int bookid = infoFromFront.getBookId();
		boolean isaddaction = infoFromFront.getAddAction();

		getConnection();
		String sql = isaddaction ? "INSERT INTO wish_list(book_id, user_id)" + " VALUES (?,?)"
				: "DELETE FROM wish_list WHERE book_id = ? and user_id = ?";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookid);
		getPstmt().setInt(2, userid);

		int rows = getPstmt().executeUpdate();

		InfoToFront info = new InfoToFront();
		info.setSuccess(rows == 1);

		closeAll();
		return info;
	}

}
