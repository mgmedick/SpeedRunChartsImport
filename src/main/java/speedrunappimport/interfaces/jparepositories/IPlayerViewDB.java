package speedrunappimport.interfaces.jparepositories;

import java.util.List;
import speedrunappimport.model.entity.*;

public interface IPlayerViewDB extends IBaseDB<PlayerView, Integer>
{
    public List<PlayerView> findByCodeIn(List<String> codes);
}
