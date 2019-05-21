package dao;

import java.sql.SQLException;

import socket.InfoFromFront;
import socket.InfoToFront;

public interface BookDao {
	/**
	 *  return the brief abstract of a book.
	 *  final edit on 5.11.2019
	 * @param infoFromFront
	 * @return bookCoverUrl, bookName, AuthorName
	 * @throws SQLException
	 */
	InfoToFront GetBookSummary(InfoFromFront infoFromFront) throws SQLException;

	/**
	 * return a part of the detail of a book
	 *
	 * @param infoFromFront
	 * @return infoToFront
	 * @throws SQLException
	 */
	InfoToFront GetBookQuasiDetail(InfoFromFront infoFromFront) throws SQLException;

	/**
	 * Get all the books that a specific user bought.
	 * Final edit in 2019.5.18. Jason Zhao.
	 *
	 * @param infoFromFront
	 * @return  Integer[]
	 * @throws SQLException
	 */
	InfoToFront GetShelfBooks(InfoFromFront infoFromFront) throws SQLException;

	/**
	 * Get all the details of a book
	 * 
	 * PublishInfo (String + String)
	 * OtherAuthor (StringBuider)
	 * 3 Booleans (Using Query in Database to check if there exists the same bookId and UserId
	 *              in the tables of wish list, read list, transaction.
	 *              if the result set is null then the user can add or buy this book.
	 *
	 * @param infoFromFront
	 * @return InfoToFront
	 *          OtherAuthors (name1(isTranlator) name2....)
	 *          Description of this book
	 *          PublishInfo "Press / publish_time / edition"
	 *          ISBN
	 *          BuyAmount, DanmuAmount, PreviewAmount, ReviewAmount, Pages of this book.
	 *          3 Booleans (
	 *          Can this user add this book to his read list or wish list.
	 *          Can this user can buy this book.
	 * @throws SQLException
	 *
	 * @param infoFromFront
	 * @return
	 * @throws SQLException
	 */
	InfoToFront GetBookDetail(InfoFromFront infoFromFront) throws SQLException;

	// get the related books, recommended books.
	InfoToFront GetRelatedBooks(InfoFromFront infoFromFront) throws SQLException;

	// preview the book, return URL
	InfoToFront GetBookPreview(InfoFromFront infoFromFront) throws SQLException;

	// download the book, return URL.
	InfoToFront DownloadBook(InfoFromFront infoFromFront) throws SQLException;

	// get the private key of a book for open the PDF.
	InfoToFront GetBookKey(InfoFromFront infoFromFront) throws SQLException;

	// issues: need to discuss with the front-end
	InfoToFront BuyBook(InfoFromFront infoFromFront) throws SQLException;

	// Check whether the contraction was done.
	InfoToFront CheckBuyComplete(InfoFromFront infoFromFront) throws SQLException;

	//
	InfoToFront CancelTransaction(InfoFromFront infoFromFront) throws SQLException;
}
