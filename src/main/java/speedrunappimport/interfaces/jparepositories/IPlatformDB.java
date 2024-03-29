package speedrunappimport.interfaces.jparepositories;

import java.util.List;
import speedrunappimport.model.entity.*;

public interface IPlatformDB extends IBaseDB<Platform, Integer>
{
    public List<Platform> findByCodeIn(List<String> codes);
}
