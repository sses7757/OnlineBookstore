package socket;

import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.sql.SQLException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import controller.AbstractController;
import controller.ReflectionController;
import dao.impl.BaseDao;

public class BookhubServer {

	/**
	 * @author Kevin Sun
	 * @param userId
	 * @param bookId
	 * @throws SQLException 
	 */
	public static void waitForPaying(final int userId, final int bookId) throws SQLException {
		// directly set paid
		String sql = "update transaction " + "set paid = true "
				+ "where transaction.user_id = ? and transaction.book_id = ?";

		new Thread(new Runnable() {
			public void run() {
				try {
					Thread.sleep(5000);

					BaseDao base = new BaseDao();
					base.getConnection();

					base.setPstmt(base.getConn().prepareStatement(sql));
					base.getPstmt().setInt(1, userId);
					base.getPstmt().setInt(2, bookId);

					base.closeAll();
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		}).run();
	}

	private static final int PORT = 2307;

	private ServerSocket server;

	/**
	 * Turn on the Server.<br>
	 * Use socket server to build the listening port.<br>
	 * Get the information from the front.<br>
	 * Invoke the controller section to deal with the information and take action.<br>
	 * Get the information from the action that we have done.(from the database)<br>
	 * Return the info in the form of Json.<br>
	 * <p>
	 * Edit on 2019.5.19 Jason Zhao.<br>
	 * Edit on 2019.5.20 Kevin Sun.<br>
	 *
	 * @throws Exception
	 */
	public void turnOnServer(final int N) throws Exception {
		// 监听指定的端口
		server = new ServerSocket(PORT);

		System.out.println("Big brother is watching.");

		ExecutorService threadPool = Executors.newFixedThreadPool(N);

		while (true) {
			Socket socket = server.accept();

			Runnable runnable = () -> {
				try {
					// 建立好连接后，从socket中获取输入流，并建立缓冲区进行读取
					InputStream inputStream = socket.getInputStream();
					byte[] bytes = new byte[1 << 14];
					int len;
					StringBuilder sb = new StringBuilder();
					while ((len = inputStream.read(bytes)) != -1) {
						// 注意指定编码格式，发送方和接收方一定要统一，建议使用UTF-8
						sb.append(new String(bytes, 0, len, "UTF-8"));
					}
					System.out.println("Get message from client:\n" + sb);
					inputStream.close();

					AbstractController controller = new ReflectionController();
					String send = controller.methodController(sb.toString());

					OutputStream outputStream = socket.getOutputStream();
					outputStream.write(send.getBytes());
					outputStream.flush();
					outputStream.close();

					socket.close();
				} catch (Exception e) {
					e.printStackTrace();
				}
			};

			threadPool.submit(runnable);
		}
	}
}
