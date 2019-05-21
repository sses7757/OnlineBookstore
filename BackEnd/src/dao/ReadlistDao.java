package dao;

import socket.InfoToFront;
import socket.InfoFromFront;

import java.sql.SQLException;

public interface ReadlistDao {
    // get the book's id from a user's readlist
    InfoToFront GetMyReadlist(InfoFromFront infoFromFront) throws SQLException;

    // get the details of this readlist.
    InfoToFront GetTitleDescription(InfoFromFront infoFromFront) throws SQLException;

    // issues : need to dicuss with the front.
    InfoToFront ChangeReadList(InfoFromFront infoFromFront) throws SQLException;

    // user can create there own readList.
    InfoToFront CreateReadList(InfoFromFront infoFromFront) throws SQLException;

    // follow or cancel following a readlist.
    InfoToFront FollowReadList(InfoFromFront infoFromFront) throws SQLException;
}
