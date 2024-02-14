package speedrunappimport.configuration;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.InjectionPoint;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.context.annotation.Configuration;
import org.springframework.context.annotation.PropertySource;
import org.springframework.context.annotation.Scope;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.repository.GameRepository;
import speedrunappimport.services.*;

@Configuration
@PropertySource("classpath:/appsettings.properties")
@ComponentScan(value={"speedrunappimport"})
public class DIConfiguration
{
	@Bean
	@Scope("prototype")
	Logger logger(InjectionPoint injectionPoint){
		return LoggerFactory.getLogger(injectionPoint.getMethodParameter().getContainingClass());
	}	

	@Bean
	public IGameRepository gameRepository(){
		return new GameRepository();
	}

	@Bean
	public IGameService getGameService(IGameRepository gameRepository, Logger logger){
		return new GameService(gameRepository, logger);
	}
}
