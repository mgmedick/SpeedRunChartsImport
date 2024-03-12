package speedrunappimport;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class Program {

	public static void main(String[] args) {
		var context = SpringApplication.run(Program.class, args);
		var app = context.getBean(Processor.class);
		app.Run();
		context.close();
	}
}
