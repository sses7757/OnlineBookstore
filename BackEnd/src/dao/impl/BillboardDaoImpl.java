package dao.impl;

import java.sql.SQLException;
import java.util.LinkedList;
import java.util.List;

import dao.BillboardDao;
import socket.InfoFromFront;
import socket.InfoToFront;

public class BillboardDaoImpl extends BaseDao implements BillboardDao {
	@Override
	public InfoToFront GetBookListBooks(InfoFromFront infoFromFront) throws SQLException {
		boolean isbillboard = infoFromFront.getBillboard();

		int booklistid = infoFromFront.getBookListID();
		int from = infoFromFront.getFrom();
		int count = infoFromFront.getCount();

		List<Integer> books = new LinkedList<Integer>();

		String sql = String.format("select bk.book_id as booklist" + " from %1$s l"
				+ " join %1$s_book bk on l.id = bk.%1$s_id" + " where l.id = ?" + " limit ? offset ?",
				isbillboard ? "billboard" : "readlist");

		getConnection();
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, booklistid);
		getPstmt().setInt(2, count);
		getPstmt().setInt(3, from);
		rs = getPstmt().executeQuery();

		while (rs.next()) {
			books.add(rs.getInt("booklist"));
		}

		closeAll();

		InfoToFront info = new InfoToFront();
		info.setIDs(books);
		return info;
	}

	@Override
	public InfoToFront GetTitleDescription(InfoFromFront infoFromFront) throws SQLException {
		boolean isbillboard = infoFromFront.getBillboard();
		int booklistid = infoFromFront.getBookListID();
		int userid = infoFromFront.getUserId();

		InfoToFront info = new InfoToFront();

		getConnection();

		if (isbillboard == true) {
			String sql = "select title, description from billboard where id = ? ";
			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setInt(1, booklistid);
			rs = getPstmt().executeQuery();
			while (rs.next()) {
				info.setTitle(rs.getString("title"));
				info.setDescription(rs.getString("description"));
			}
		}
		else {
			String sql = "select r.title, r.create_user, r.description, r.edit_time,"
					+ " follow_amount(r.id) as followamount, (rf.user_id = ? as isfollowed)" + " from readlist r"
					+ " join readlist_follow rf on r.id = rf.readlist_id" + " where id = ?" + "group by isfollowed"
					+ " order by isfollowed desc limit 1";
			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setInt(1, userid);
			getPstmt().setInt(2, booklistid);

			rs = getPstmt().executeQuery();

			while (rs.next()) {
				info.setTitle(rs.getString("title"));
				info.setDescription(rs.getString("description"));
				info.setUserId(rs.getInt("create_user"));
				info.setTimeStap(rs.getLong("edit_time"));
				info.setFollowAmount(rs.getInt("followamount"));
				info.setFollowed(rs.getBoolean("isfollowed"));
			}
		}

		closeAll();

		return info;
	}
}
