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
	public ISettingRepository getSettingRepository(ISettingDB settingDB, Logger logger){
		return new SettingRepository(settingDB, logger);
	}

	@Bean
	public IPlayerRepository getPlayerRepository(IPlayerDB playerDB, IPlayerViewDB playerViewDB, IPlayerLinkDB playerLinkDB, Logger logger){
		return new PlayerRepository(playerDB, playerViewDB, playerLinkDB, logger);
	}

	@Bean
	public IPlatformRepository getPlatformRepository(IPlatformDB platformDB, Logger logger){
		return new PlatformRepository(platformDB, logger);
	}

	@Bean
	public IGameRepository getGameRepository(IGameDB gameDB, IGameViewDB gameViewDB, IGameLinkDB gameLinkDB, ICategoryDB categoryDB, ILevelDB levelDB, IVariableDB variableDB, IVariableValueDB variableValueDB, IGamePlatformDB gamePlatformDB, IGameCategoryTypeDB gameCategoryTypeDB, Logger logger){
		return new GameRepository(gameDB, gameViewDB, gameLinkDB, categoryDB, levelDB, variableDB, variableValueDB, gamePlatformDB, gameCategoryTypeDB, logger);
	}

	@Bean
	public ISpeedRunRepository getSpeedRunRepository(ISpeedRunDB speedRunDB, ISpeedRunViewDB speedRunViewDB, ISpeedRunLinkDB speedRunLinkDB, ISpeedRunPlayerDB speedRunPlayerDB, ISpeedRunVariableValueDB speedRunVariableValueDB, ISpeedRunVideoDB speedRunVideoDB, Logger logger){
		return new SpeedRunRepository(speedRunDB, speedRunViewDB, speedRunLinkDB, speedRunPlayerDB, speedRunVariableValueDB, speedRunVideoDB, logger);
	}

	@Bean
	public ISettingService getSettingService(ISettingRepository settingRepo, Logger logger){
		return new SettingService(settingRepo, logger);
	}

	@Bean
	public IPlatformService getPlatformService(IPlatformRepository platformRepo, Logger logger){
		return new PlatformService(platformRepo, logger);
	}

	@Bean
	public IGameService getGameService(IGameRepository gameRepo, IPlatformRepository platformRepo, ISettingService settingService, Logger logger){
		return new GameService(gameRepo, platformRepo, settingService, logger);
	}

	@Bean
	public ISpeedRunService getSpeedRunService(ISpeedRunRepository speedRunRepo, IGameRepository gameRepo, IPlatformRepository platformRepo, IPlayerRepository playerRepo, ISettingService settingService, Logger logger){
		return new SpeedRunService(speedRunRepo, gameRepo, platformRepo, playerRepo, settingService, logger);
	}	
}
