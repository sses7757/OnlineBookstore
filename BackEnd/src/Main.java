import socket.BookhubServer;

public class Main {

	public static void main(String[] args) {
		BookhubServer bookhubServer = new BookhubServer();
		try {
			bookhubServer.turnOnServer();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
}
