package dao.impl;

import java.sql.SQLException;

import dao.ReadlistDao;
import socket.InfoFromFront;
import socket.InfoToFront;
import socket.frontEnum.BookListChangeType;

public class ReadlistDaoImpl extends BaseDao implements ReadlistDao {
	@Override
	public InfoToFront GetMyReadlist(InfoFromFront infoFromFront) {
		int userId, from, count;
		userId = infoFromFront.getUserId();
		from = infoFromFront.getFrom();
		count = infoFromFront.getCount();
		return null;
	}

	@Override
	public InfoToFront GetTitleDescription(InfoFromFront infoFromFront) {
		int readlistId = infoFromFront.getReadListId();
		return null;
	}

	@Override
	public InfoToFront ChangeReadList(InfoFromFront infoFromFront) {
		int readListId, alteredBookId;
		BookListChangeType changeType = BookListChangeType.values()[infoFromFront.getChangeType()];
		String alteredText;
		readListId = infoFromFront.getReadListId();
		alteredBookId = infoFromFront.getAlteredBookId();
		return null;
	}

	@Override
	public InfoToFront CreateReadList(InfoFromFront infoFromFront) throws SQLException {
		int userId = infoFromFront.getUserId();
		String title = infoFromFront.getTitle();
		String description = infoFromFront.getDescription();

		return null;
	}

	@Override
	public InfoToFront FollowReadList(InfoFromFront infoFromFront) throws SQLException {
		int UserId = infoFromFront.getUserId();
		boolean isFollowAction = infoFromFront.getIsFollowAction();
		int readlistId;
		return null;
	}

}
