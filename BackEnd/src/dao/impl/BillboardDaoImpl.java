package dao.impl;

import dao.BillboardDao;
import socket.InfoToFront;
import socket.InfoFromFront;

import java.sql.SQLException;

public class BillboardDaoImpl extends BaseDao implements BillboardDao {
    @Override
    public InfoToFront GetBillboardList(InfoFromFront infoFromFront) throws SQLException {
        return null;
    }

    @Override
    public InfoToFront GetTitleDescription(InfoFromFront infoFromFront) throws SQLException {
        return null;
    }
}
