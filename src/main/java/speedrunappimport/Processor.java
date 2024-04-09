package speedrunappimport;

import org.springframework.stereotype.Service;

import jakarta.persistence.criteria.CriteriaBuilder.In;

import org.springframework.beans.factory.annotation.Value;

import java.time.Instant;
import org.slf4j.Logger;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.model.entity.Setting;

@Service
public class Processor {
	private IPlatformService _platformService;
	private IGameService _gameService;
	private ISettingService _settingService;	
	private Logger _logger;
	// private Logger _logger = LoggerFactory.getLogger(GameService.class);
	// private Environment _env;

	public Processor(IPlatformService platformService, IGameService gameService, ISettingService settingService, Logger logger) {
		this._platformService = platformService;
		this._gameService = gameService;
		this._settingService = settingService;
		this._logger = logger;
		// this._env = env;
	}

	public void Run() {
		try {
			Init();
		} catch (Exception ex) {
			_logger.error("Run", ex);
		}
	}

	public void Init() {
		try {
			_logger.info("Started Init");
			
			if (this.isReload()) {
				_platformService.ProcessPlatforms();
			}

			var stGameLastImportRefDateUtc = _settingService.GetSetting("GameLastImportRefDate");
			var gameLastImportRefDateUtc = stGameLastImportRefDateUtc != null ? stGameLastImportRefDateUtc.getDte() : this.getSqlMinDateTime();
			_gameService.ProcessGames(gameLastImportRefDateUtc, this.isReload());

			_settingService.UpdateSetting("LastImportDateUtc", Instant.now());
			_logger.info("Completed Init");
		} catch (Exception ex) {
			_logger.error("Run", ex);
		}
	}

	@Value("${isreload}")
	private boolean isReload;

	@Value("${settings.sqlMinDateTime}")
	private Instant sqlMinDateTime;

	public boolean isReload() {
		return isReload;
	}

	public void setReload(boolean isReload) {
		this.isReload = isReload;
	}

	public Instant getSqlMinDateTime() {
		return sqlMinDateTime;
	}

	public void setSqlMinDateTime(Instant sqlMinDateTime) {
		this.sqlMinDateTime = sqlMinDateTime;
	}
}
