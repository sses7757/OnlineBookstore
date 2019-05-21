package dao.impl;

import java.sql.SQLException;
import java.util.LinkedList;
import java.util.List;

import dao.LabelDao;
import socket.InfoFromFront;
import socket.InfoToFront;

public class LabelDaoImpl extends BaseDao implements LabelDao {

	@Override
	public InfoToFront GetMainLabels(InfoFromFront infoFromFront) throws SQLException {
		List<String> labelList = new LinkedList<String>();
		getConnection();

		String sql = "select name from label l";
		pstmt = conn.prepareStatement(sql);
		rs = pstmt.executeQuery();

		while (rs.next()) {
			labelList.add(rs.getString("name"));
		}

		closeAll();
		InfoToFront info = new InfoToFront();
		info.setMainLabels(labelList);
		return info;
	}

	@Override
	public InfoToFront GetSubLabels(InfoFromFront infoFromFront) throws SQLException {
		String mainLabels = infoFromFront.getMainLabel();
		List<String> labelList = new LinkedList<String>();

		getConnection();

		String sql = "selct sl.name" + "( avg (bs.buys)*0.8 + avg(reviews)*0.1 + avg(danmus)*0.1 ) as hot_spot "
				+ "from sub_label sl" + "join book b on b.id = sl.id" + "join book_stat bs on bs.id = b.id"
				+ "join label l on sl.id = l.main_id" + "group by l.id" + "order by hot_spot desc"
				+ "where l.name = ?";

		pstmt.setString(1, mainLabels);
		pstmt = conn.prepareStatement(sql);
		rs = pstmt.executeQuery();

		while (rs.next()) {
			labelList.add(rs.getString("sl.name"));
		}

		closeAll();

		InfoToFront info = new InfoToFront();
		info.setSubLabels(labelList);
		return info;
	}
}
