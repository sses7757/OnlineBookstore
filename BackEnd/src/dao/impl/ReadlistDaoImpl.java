package dao.impl;

import java.sql.SQLException;
import java.util.LinkedList;
import java.util.List;

import dao.ReadlistDao;
import socket.InfoFromFront;
import socket.InfoToFront;
import socket.frontEnum.ReadListChangeType;

public class ReadlistDaoImpl extends BaseDao implements ReadlistDao {

	@Override
	public InfoToFront GetMyCreatedReadLists(InfoFromFront infoFromFront) throws SQLException {
		int userId, from, count;
		userId = infoFromFront.getUserId();
		from = infoFromFront.getFrom();
		count = infoFromFront.getCount();
		List<Integer> myreadlist = new LinkedList<>();

		getConnection();

		String sql = "select .book_id" + " from readlist r" + " where create_user = ?" + " limit ? offset ?";
		setPstmt(getConn().prepareStatement(sql));

		getPstmt().setInt(1, userId);
		getPstmt().setInt(2, count);
		getPstmt().setInt(3, from);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			myreadlist.add(rs.getInt("book_id"));

		}

		closeAll();

		InfoToFront info = new InfoToFront();
		info.setIDs(myreadlist);
		return info;
	}

	@Override
	public InfoToFront GetMyFollowedReadLists(InfoFromFront infoFromFront) throws SQLException {
		int userId, from, count;
		userId = infoFromFront.getUserId();
		from = infoFromFront.getFrom();
		count = infoFromFront.getCount();
		List<Integer> myreadlist = new LinkedList<>();

		getConnection();

		String sql = "select r.id" + " from readlist r" + " join readlist_follow rf on r.id = rf.readlist_id"
				+ " where rf.user_id = ?" + " limit ? offset ?;";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, userId);
		getPstmt().setInt(2, count);
		getPstmt().setInt(3, from);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			myreadlist.add(rs.getInt("id"));
		}

		closeAll();

		InfoToFront info = new InfoToFront();
		info.setIDs(myreadlist);
		return info;
	}

	@Override
	public InfoToFront ChangeReadList(InfoFromFront infoFromFront) throws SQLException {
		int readListId, alteredBookId;
		ReadListChangeType changeType = ReadListChangeType.values()[infoFromFront.getChangeType()];
		String alteredText;
		readListId = infoFromFront.getReadListId();
		alteredBookId = infoFromFront.getAlteredBookId();
		alteredText = infoFromFront.getAlteredText();

		getConnection();

		String sql = "";
		switch (changeType) {
		case AddBook:
			sql += "UPDATE readlist " + "SET book_id = ? where readlist = ?;";
			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setInt(1, alteredBookId);
			getPstmt().setInt(2, readListId);
			break;
		case RemoveList:
			sql += "delete from readlist where id = ?;";
			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setInt(1, readListId);
			break;
		case DeleteBook:
			sql += "delete from readlist_books where readlist_id = ? and book_id = ?;";
			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setInt(1, readListId);
			getPstmt().setInt(2, alteredBookId);
			break;
		case ChangeDescription:
			sql += "UPDATE readlist " + "SET description = ? where readlist = ?;";
			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setString(1, alteredText);
			getPstmt().setInt(2, readListId);
			break;
		case ChangeTitle:
			sql += "UPDATE readlist " + "SET title = ? where readlist = ?;";
			setPstmt(getConn().prepareStatement(sql));
			getPstmt().setString(1, alteredText);
			getPstmt().setInt(2, readListId);
			break;
		default:
			break;
		}

		int rows = getPstmt().executeUpdate();
		InfoToFront info = new InfoToFront();
		info.setSuccess(rows == 1);

		closeAll();
		return info;
	}

	@Override
	public InfoToFront CreateReadList(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		String title = infoFromFront.getTitle();
		String description = infoFromFront.getDescription();

		String sql = "insert into readlsit (create_user, title, description) values (?,?,?);";
		getConnection();

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, userId);
		getPstmt().setString(2, title);
		getPstmt().setString(3, description);

		int rows = getPstmt().executeUpdate();
		InfoToFront info = new InfoToFront();
		info.setSuccess(rows == 1);

		closeAll();
		return info;
	}

	@Override
	public InfoToFront FollowReadList(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		boolean isFollowAction = infoFromFront.getIsFollowAction();
		int readlistId = infoFromFront.getReadListId();

		String sql = "";

		if (isFollowAction) {
			sql += "insert into readlist_follow(user_id, readlist_id) values (?,?);";
		}
		else {
			sql += "delete from readlist_follow where user_id = ? and readlist_id = ?;";
		}

		getConnection();
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, userId);
		getPstmt().setInt(2, readlistId);

		int rows = getPstmt().executeUpdate();
		InfoToFront info = new InfoToFront();
		info.setSuccess(rows == 1);

		closeAll();
		return info;
	}
}
