import socket.BookhubServer;

public class Main {

	public static void main(String[] args) throws InterruptedException {

		Thread userThread = new Thread(new Runnable() {
			@Override
			public void run() {
				BookhubServer userServer = new BookhubServer();
				try {
					userServer.turnOnServer(20);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});

		Thread adminThread = new Thread(new Runnable() {
			@Override
			public void run() {
				BookhubServer adminServer = new BookhubServer(true);
				try {
					adminServer.turnOnServer(20);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});

		userThread.start();
		adminThread.start();
		userThread.join();
		adminThread.join();
	}
}
