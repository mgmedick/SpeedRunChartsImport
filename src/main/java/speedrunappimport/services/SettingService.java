package speedrunappimport.services;

import java.time.Instant;

import org.slf4j.Logger;

import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.model.entity.*;

public class SettingService extends BaseService implements ISettingService {
	private ISettingRepository _settingRepo;
	private Logger _logger;

	public SettingService(ISettingRepository settingRepo, Logger logger) {
		_settingRepo = settingRepo;
		_logger = logger;
	}
	public Setting GetSetting(String name)
	{
		return _settingRepo.GetSetting(name);
	}

	public void UpdateSetting(String name, Instant value)
	{
		var setting = _settingRepo.GetSetting(name);
		setting.setDte(value);
		_settingRepo.SaveSetting(setting);
	}

	public void UpdateSetting(String name, String value)
	{
		var setting = _settingRepo.GetSetting(name);
		setting.setStr(value);
		_settingRepo.SaveSetting(setting);
	}

	public void UpdateSetting(String name, Integer value)
	{
		var setting = _settingRepo.GetSetting(name);
		setting.setNum(value);
		_settingRepo.SaveSetting(setting);
	}

	public void UpdateSetting(Setting setting)
	{
		_settingRepo.SaveSetting(setting);
	}
}
