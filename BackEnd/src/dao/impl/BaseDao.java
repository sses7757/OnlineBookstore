package dao.impl;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;

public class BaseDao {
	protected Connection conn;
	protected PreparedStatement pstmt;
	protected ResultSet rs;
	// protected int result;

	// JDBC driver and database URL
	static final String JDBC_DRIVER = "com.mysql.cj.jdbc.Driver";
	static final String DB_URL = "jdbc:mysql://localhost:3306/bookstore?serverTimezone=UTC&useSSL=false";
	// user name and password.
	static final String USER = "root";
	static final String PASS = "112233";

	static {
		try {
			Class.forName(JDBC_DRIVER);
		} catch (ClassNotFoundException e) {
			e.printStackTrace();
			System.err.println("Fail to load the database");
		}
	}

	/**
	 * Build the connection with the database
	 */
	public void getConnection() {

		try {
			setConn(DriverManager.getConnection(DB_URL, USER, PASS));
			// Since the default of MySQL is repeatable-read, there is no need
			// pstmt = conn.prepareStatement("set session transaction isolation level repeatable read;");
			// pstmt.execute();
			pstmt = conn.prepareStatement("set global sql_safe_updates = 1;");
			pstmt.execute();
		} catch (SQLException e) {
			e.printStackTrace();
			System.err.println("Fail to connect to the database. "
					+ "Please check the service, database status and user's account.");
		}
	}

	/**
	 * close all the database connection
	 */
	public void closeAll() {
		try {
			if (getPstmt() != null && !getPstmt().isClosed()) {
				getPstmt().close();
			}
			if (rs != null && !rs.isClosed()) {
				rs.close();
			}
			if (getConn() != null && !getConn().isClosed()) {
				getConn().close();
			}
		} catch (SQLException e) {
			e.printStackTrace();
			System.err.println("Exception occurs when closing the database connection.");
		}
	}

	public PreparedStatement getPstmt() {
		return pstmt;
	}

	public void setPstmt(PreparedStatement pstmt) {
		this.pstmt = pstmt;
	}

	public Connection getConn() {
		return conn;
	}

	public void setConn(Connection conn) {
		this.conn = conn;
	}
}
