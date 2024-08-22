package speedrunappimport;

import org.springframework.stereotype.Service;
import org.springframework.core.env.Environment;

import java.time.Instant;
import org.slf4j.Logger;
import speedrunappimport.interfaces.services.*;

@Service
public class Processor {
	private IPlatformService _platformService;
	private IGameService _gameService;
	private ISpeedRunService _speedRunService;
	private ISettingService _settingService;	
	private Logger _logger;
	private Environment _env;
	// private Logger _logger = LoggerFactory.getLogger(GameService.class);

	public Processor(IPlatformService platformService, IGameService gameService, ISpeedRunService speedRunService, ISettingService settingService, Environment env, Logger logger) {
		this._platformService = platformService;
		this._gameService = gameService;
		this._speedRunService = speedRunService;
		this._settingService = settingService;
		this._logger = logger;
		this._env = env;
	}

	public void Run() {
		try {
			Init();
			RunProcesses();
		} catch (Exception ex) {
			_logger.error("Run", ex);
		}
	}

	public void Init() {
		try {
			_logger.info("Started Init");
			
			this.setIsReload(_env.getProperty("isReload", boolean.class));
		
		} catch (Exception ex) {
			_logger.error("Run", ex);
		}
	}

	public void RunProcesses() {
		try {
			var result = false;
			_logger.info("Started RunProcesses");

			result = _platformService.ProcessPlatforms();
			
			if (result) {
				result = _gameService.ProcessGames(this.isReload());
			}

			if (result) {
				result = _speedRunService.ProcessSpeedRuns(this.isReload());
			}		
			
			_settingService.UpdateSetting("LastImportDate", Instant.now());
			_logger.info("Completed RunProcesses");
		} catch (Exception ex) {
			_logger.error("Run", ex);
		}
	}

	private boolean isReload;

	public boolean isReload() {
		return isReload;
	}

	public void setIsReload(boolean isReload) {
		this.isReload = isReload;
	}	
}
