package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface ISpeedRunViewDB extends IBaseDB<SpeedRunView, Integer>
{
    public List<SpeedRunView> findByCodeIn(List<String> codes);
}
