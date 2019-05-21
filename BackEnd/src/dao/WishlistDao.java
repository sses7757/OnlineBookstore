package dao;

import socket.InfoToFront;
import socket.InfoFromFront;

import java.sql.SQLException;

public interface WishlistDao {

    // get the books' id from a user's wishlist
    InfoToFront GetMyWishlist(InfoFromFront infoFromFront) throws SQLException;

}
