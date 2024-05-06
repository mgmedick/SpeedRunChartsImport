package speedrunappimport.interfaces.jparepositories;

import speedrunappimport.model.entity.*;

public interface ISettingDB extends IBaseDB<Setting, Integer>
{
    public Setting getByName(String name);
}
