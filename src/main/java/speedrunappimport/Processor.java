package speedrunappimport;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.time.Instant;

import org.slf4j.Logger;
import speedrunappimport.interfaces.services.*;

@Service
public class Processor {
	private IGameService _gameService;
	private Logger _logger;
	//private Logger _logger = LoggerFactory.getLogger(GameService.class);
	//private Environment _env;

	public Processor(IGameService gameService, Logger logger)
	{
		this._gameService = gameService;
		this._logger = logger;
		//this._env = env;
	}
	
	public void Run()
	{
		try
		{
			Init();
		}
		catch (Exception ex)
		{
			_logger.error("Run", ex);
		}
	}

	public void Init()
	{
		try
		{
			_logger.info("Started Init");
			_gameService.ProcessGames(Instant.now(), false);
			_logger.info("Completed Init");
		}
		catch (Exception ex)
		{
			_logger.error("Run", ex);
		}
	}
	
	@Value("${isreload}")
	private boolean isReload;

	@Value("${database.ConnectionString}")
	private String connString;		

	@Value("${settings.pullDelayMS}")
	private String pullDelayMS;	
}
