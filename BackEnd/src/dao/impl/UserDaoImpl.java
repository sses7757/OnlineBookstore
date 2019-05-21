package dao.impl;

import java.sql.SQLException;

import dao.UserDao;
import socket.InfoFromFront;
import socket.InfoToFront;
import socket.frontEnum.LoginStatus;

public class UserDaoImpl extends BaseDao implements UserDao {

	public InfoToFront Login(InfoFromFront infoFromFront) throws SQLException {
		String userName = infoFromFront.getUserName();
		String encodedPassword = infoFromFront.getEncodedPassword();

		InfoToFront info = new InfoToFront();

		getConnection();

		String sql = "select id, name, password_encode, authority from user u where u.name = ? ";
		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setString(1, userName);
		rs = getPstmt().executeQuery();
		if (rs.next()) {
			String password = rs.getString("password_encode").trim();
			int userId = rs.getInt("id");
			Boolean isAdmin = rs.getBoolean("authority");

			if (password.equals(encodedPassword)) {
				info.setLoginStatus(LoginStatus.Success);
				info.setUserId(userId);
				info.setAdmin(isAdmin);
			}
			else {
				info.setLoginStatus(LoginStatus.WrongPassword);
			}
		}
		else {
			info.setLoginStatus(LoginStatus.NoSuchUser);
		}

		closeAll();

		return info;
	}

	@Override
	public InfoToFront SignUp(InfoFromFront infoFromFront) throws SQLException {
		String userName = infoFromFront.getUserName();
		String mailAddr = infoFromFront.getEmail();
		String encodedPassword = infoFromFront.getEncodedPassword();

		InfoToFront info = new InfoToFront();

		String sql = "INSERT INTO user (name, email, password_encode) value(?, ?, ?) ";

		getConnection();

		setPstmt(getConn().prepareStatement(sql));
		getPstmt().setString(1, userName);
		getPstmt().setString(2, mailAddr);
		getPstmt().setString(3, encodedPassword);

		int rows = getPstmt().executeUpdate();

		if (rows == 1)
			info.setSuccess(true);
		else
			info.setSuccess(false);

		closeAll();

		return info;
	}
}
