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
	public IPlatformRepository getPlatformRepository(IPlatformDB platformDB, Logger logger){
		return new PlatformRepository(platformDB, logger);
	}

	@Bean
	public IGameRepository getGameRepository(IGameDB gameDB, IGameViewDB gameViewDB, ICategoryDB categoryDB, ILevelDB levelDB, IVariableDB variableDB, IVariableValueDB variableValueDB, IGamePlatformDB gamePlatformDB, ICategoryTypeDB categoryTypeDB, IGameCategoryTypeDB gameCategoryTypeDB, Logger logger){
		return new GameRepository(gameDB, gameViewDB, categoryDB, levelDB, variableDB, variableValueDB, gamePlatformDB, categoryTypeDB, gameCategoryTypeDB, logger);
	}

	@Bean
	public IGameService getGameService(IGameRepository gameRepository, IPlatformRepository platformRepository, Logger logger){
		return new GameService(gameRepository, platformRepository, logger);
	}
}
