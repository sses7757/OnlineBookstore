package dao.impl;

import dao.WishlistDao;
import socket.InfoToFront;
import socket.InfoFromFront;

import java.sql.SQLException;

public class WishlistDaoImpl extends BaseDao implements WishlistDao {
    @Override
    public InfoToFront GetMyWishlist(InfoFromFront infoFromFront) throws SQLException {
        int userId = infoFromFront.getUserId();
        return null;
    }
}
