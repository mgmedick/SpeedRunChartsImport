package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface ISpeedRunDB extends IBaseDB<SpeedRun, Integer>
{
    public List<SpeedRun> findByCodeIn(List<String> codes);
}
