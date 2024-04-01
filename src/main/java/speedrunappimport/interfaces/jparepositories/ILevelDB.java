package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface ILevelDB extends IBaseDB<Level, Integer> 
{
    public List<Level> findByCodeIn(List<String> codes);   
}
