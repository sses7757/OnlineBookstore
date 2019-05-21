import socket.BookhubServer;

public class Main {

	public static void main(String[] args) {
		BookhubServer bookhubServer = new BookhubServer();
		try {
			bookhubServer.turnOnServer(20);
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
}
