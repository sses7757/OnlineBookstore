package dao.impl;

import java.sql.SQLException;
import java.util.LinkedList;
import java.util.List;

import dao.DanmuDao;
import socket.InfoFromFront;
import socket.InfoToFront;

public class DanmuDaoImpl extends BaseDao implements DanmuDao {

	@Override
	public InfoToFront GetDanmuContent(InfoFromFront infoFromFront) throws SQLException {
		int danmuId = infoFromFront.getDanmuId();

		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "select content from danmu where danmu.id = ? ";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, danmuId);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			info.setContent(rs.getString("content"));
		}

		closeAll();

		return info;
	}

	@Override
	public InfoToFront GetMyDanmus(InfoFromFront infoFromFront) throws SQLException {
		int userId, from, count;
		userId = infoFromFront.getUserId();
		from = infoFromFront.getFrom();
		count = infoFromFront.getCount();

		List<Integer> mydanmu = new LinkedList<>();
		getConnection();

		String sql = "select id from danmu where user_id = ? limit ? offset ?";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, userId);
		getPstmt().setInt(2, count);
		getPstmt().setInt(3, from);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			mydanmu.add(rs.getInt("id"));
		}
		closeAll();

		InfoToFront info = new InfoToFront();
		info.setIDs(mydanmu);
		return info;

	}

	@Override
	public InfoToFront GetDanmuOfBook(InfoFromFront infoFromFront) throws SQLException {
		int bookId, page;
		bookId = infoFromFront.getBookId();
		page = infoFromFront.getPage();
		List<Integer> danmuofbook = new LinkedList<>();
		getConnection();

		String sql = "select id from danmu where book_id = ? and page_num = ?";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, bookId);
		getPstmt().setInt(2, page);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			danmuofbook.add(rs.getInt("id"));
		}

		closeAll();

		InfoToFront info = new InfoToFront();
		info.setIDs(danmuofbook);
		return info;
	}

	@Override
	public InfoToFront GetFullDanmuContent(InfoFromFront infoFromFront) throws SQLException {
		int danmuId = infoFromFront.getDanmuId();
		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "select d.content, d.page_num, b.name" + " from danmu d" + " join book b on d.book_id = b.id"
				+ " where d.id = ?";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, danmuId);

		rs = getPstmt().executeQuery();

		while (rs.next()) {
			info.setContent(rs.getString("content"));
			info.setBookName(rs.getString("name"));
			info.setPageNum(rs.getInt("page_num"));
		}

		closeAll();

		return info;
	}

	@Override
	public InfoToFront ChangeDanmu(InfoFromFront infoFromFront) throws SQLException {
		int danmuId = infoFromFront.getDanmuId();
		boolean isDeleteAction = infoFromFront.getDeleteAction();
		String newContent = infoFromFront.getNewContent();

		InfoToFront info = new InfoToFront();

		getConnection();
		String sql = "";
		if (isDeleteAction) {
			sql += "DELETE FROM danmu d" + " WHERE d.id = ?";
		}
		else {
			sql += "UPDATE danmu d" + " SET content = ?" + " WHERE d.id = ?";
		}

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setString(1, newContent);
		getPstmt().setInt(2, danmuId);

		int rows = getPstmt().executeUpdate();
		info.setSuccess(rows == 1);

		closeAll();

		return info;
	}

	@Override
	public InfoToFront CreateDanmu(InfoFromFront infoFromFront) throws SQLException {
		String content;
		int bookId, userId, pageNum;
		content = infoFromFront.getContent();
		bookId = infoFromFront.getBookId();
		userId = infoFromFront.getUserId();
		pageNum = infoFromFront.getPageNum();

		InfoToFront info = new InfoToFront();

		getConnection();
		String sql = null;

		sql = "insert into  danmu(user_id, book_id, page_num, content) values(?,?,?,?)";

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setInt(1, userId);
		getPstmt().setInt(2, bookId);
		getPstmt().setInt(3, pageNum);
		getPstmt().setString(4, content);

		setPstmt(getConn().prepareStatement(sql));
		int rows = getPstmt().executeUpdate();
		info.setSuccess(rows == 1);

		closeAll();
		return info;

	}
}
