package dao.impl;

import java.sql.SQLException;

import dao.DanmuDao;
import socket.InfoFromFront;
import socket.InfoToFront;

public class DanmuDaoImpl extends BaseDao implements DanmuDao {

	public InfoToFront GetDanmuContent(InfoFromFront infoFromFront) throws SQLException {
		int danmuId = infoFromFront.getDanmuId();
		InfoToFront dataToFront = new InfoToFront();
		String content = null;
		getConnection();

		String sql = "select content from danmu where danmu.id = ? ";
		pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, danmuId);

		rs = pstmt.executeQuery();

		while (rs.next()) {
			content = rs.getString("content");
			dataToFront.setContent(content);
		}

		closeAll();

		return dataToFront;
	}

	@Override
	public InfoToFront GetMyDanmus(InfoFromFront infoFromFront) throws SQLException {
		int userId, from, count;
		userId = infoFromFront.getUserId();
		from = infoFromFront.getFrom();
		count = infoFromFront.getCount();

		return null;
	}

	@Override
	public InfoToFront GetDanmuOfBook(InfoFromFront infoFromFront) throws SQLException {
		int bookId, page, limit;
		bookId = infoFromFront.getBookId();
		page = infoFromFront.getPage();

		return null;
	}

	@Override
	public InfoToFront GetFullDanmuContent(InfoFromFront infoFromFront) throws SQLException {
		int danmuId = infoFromFront.getDanmuId();
		return null;
	}

	@Override
	public InfoToFront ChangeDanmu(InfoFromFront infoFromFront) throws SQLException {
		int danmuId = infoFromFront.getDanmuId();
		boolean isDeleteAction = infoFromFront.getDeleteAction();
		String newContent = infoFromFront.getNewContent();
		InfoToFront info = new InfoToFront();

		getConnection();
		String sql = null;
		if (isDeleteAction) {
			sql = "DELETE FROM danmu d WHERE d.id = ?";
		}
		else {
			sql = "set sql_safe_updates = 1" + " UPDATE danmu d" + "SET content = ?" + "WHERE d.id = ?";
		}

		pstmt = conn.prepareStatement(sql);
		int rows = pstmt.executeUpdate();
		if (rows == 1) {
			info.setSuccess(true);
		}
		else {
			info.setSuccess(false);
		}

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
		return null;
	}
}
