package controller;

import java.lang.reflect.Method;

import service.DAOFactory;
import service.impl.DAOFactoryImpl;
import socket.InfoFromFront;
import socket.InfoToFront;

/**
 * @author Jason Zhao
 * <p>
 * Edit on 2019.5.20 Kevin Sun.
 */
public class ReflectionController extends AbstractController {

	/*
	 * £¨·Ç Javadoc£©
	 * 
	 * @see controller.AbstractController#methodController(java.lang.String)
	 */
	@Override
	public String methodController(String infoFromFront) throws Exception {
		InfoFromFront info = this.fromJson(infoFromFront);

		DAOFactory factory = new DAOFactoryImpl();

		String type = info.getType();

		// According to the method, find the corresponding object. baseDao <- son of BaseDao.
		Object baseDao = factory.getBaseDao(type);

		Method method = null;
		Method[] methods = baseDao.getClass().getMethods();
		for (int i = 0; i < methods.length; i++) {
			if (methods[i].getName().equalsIgnoreCase(type))
				method = methods[i];
		}

		Object infoToFront = method.invoke(baseDao, info);
		InfoToFront front;

		if (infoToFront instanceof InfoToFront) {
			front = (InfoToFront) infoToFront;
			front.setType(method.getName());
		}
		else {
			throw new Exception("Invoke method did not return correct result");
		}
		return this.toJson(front);
	}
}
