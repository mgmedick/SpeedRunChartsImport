package speedrunappimport.interfaces.repositories;

import speedrunappimport.model.entity.*;

public interface ISettingRepository {
    Setting GetSetting(String name);
    void SaveSetting(Setting settting);
}


