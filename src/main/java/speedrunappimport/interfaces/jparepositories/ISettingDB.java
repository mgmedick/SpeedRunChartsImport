package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface ISettingDB extends IBaseDB<Setting, Integer>
{
    public Setting getByName(String name);
}
