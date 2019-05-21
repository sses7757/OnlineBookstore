package dao;
import socket.InfoToFront;
import socket.InfoFromFront;

import java.sql.SQLException;

public interface DanmuDao {
    // get the content of a barrage by its id
    InfoToFront GetDanmuContent(InfoFromFront infoFromFront) throws SQLException;

    // get the Danmus' id that a user edited.
    InfoToFront GetMyDanmus(InfoFromFront infoFromFront) throws SQLException;

    // get the IDs of a book's Danmu
    InfoToFront GetDanmuOfBook(InfoFromFront infoFromFront) throws SQLException;

    // get full content of a Danmu
    InfoToFront GetFullDanmuContent(InfoFromFront infoFromFront) throws SQLException;

    // delete the danmu or update danmu by danmuId.
    InfoToFront ChangeDanmu(InfoFromFront infoFromFront) throws SQLException;

    // Create or send danmu.
    InfoToFront CreateDanmu(InfoFromFront infoFromFront) throws SQLException;
}
