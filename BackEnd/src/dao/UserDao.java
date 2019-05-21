package dao;

import socket.InfoToFront;
import socket.InfoFromFront;

import java.sql.SQLException;

public interface UserDao {

    // Check the user's info and return
    InfoToFront Login(InfoFromFront infoFromFront) throws SQLException;

    // get the readlists' Id (created or followed) by a user.
    InfoToFront GetMyReadList(InfoFromFront infoFromFront) throws SQLException;

    // user register.
    InfoToFront SignUp(InfoFromFront infoFromFront) throws SQLException;
}
