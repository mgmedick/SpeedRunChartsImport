package speedrunappimport.configuration;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.InjectionPoint;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.context.annotation.Configuration;
import org.springframework.context.annotation.PropertySource;
import org.springframework.context.annotation.Scope;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;
import org.springframework.transaction.annotation.EnableTransactionManagement;

import speedrunappimport.interfaces.jparepositories.*;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.repositories.*;
import speedrunappimport.services.*;

@Configuration
@PropertySource("classpath:/appsettings.properties")
@ComponentScan(value={"speedrunappimport"})
@EntityScan("speedrunappimport.model.entity")
@EnableJpaRepositories("speedrunappimport.interfaces.jparepositories")
@EnableTransactionManagement
public class DIConfiguration
{
	@Bean
	@Scope("prototype")
	Logger logger(InjectionPoint injectionPoint){
		return LoggerFactory.getLogger(injectionPoint.getMethodParameter().getContainingClass());
	}	

	@Bean
	public IGameRepository getGameRepository(IGameDB gameDB, ILevelDB levelDB){
		return new GameRepository(gameDB, levelDB);
	}

	@Bean
	public IGameService getGameService(IGameRepository gameRepository, Logger logger){
		return new GameService(gameRepository, logger);
	}
}
