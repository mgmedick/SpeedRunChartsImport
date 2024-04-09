package speedrunappimport.interfaces.services;

import java.time.Instant;

import speedrunappimport.model.entity.Setting;

public interface ISettingService
{
	Setting GetSetting(String name);
	void UpdateSetting(String name, Instant value);
	void UpdateSetting(String name, String value);
	void UpdateSetting(String name, Integer value);
	void UpdateSetting(Setting setting);
}
