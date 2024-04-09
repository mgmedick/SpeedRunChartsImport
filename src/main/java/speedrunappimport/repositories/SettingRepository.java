package speedrunappimport.repositories;

import java.util.List;

import org.slf4j.Logger;
import speedrunappimport.interfaces.jparepositories.*;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.model.entity.*;

public class SettingRepository extends BaseRepository implements ISettingRepository {
	private ISettingDB _settingDB;
	private Logger _logger;

	public SettingRepository(ISettingDB settingDB, Logger logger) {
		_settingDB = settingDB;
		_logger = logger;
	}

	public Setting GetSetting(String name)
	{
		return _settingDB.getByName(name);
	}

	public void SaveSetting(Setting setting)
	{
		_settingDB.save(setting);
	}
}
