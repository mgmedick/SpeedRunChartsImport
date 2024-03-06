package speedrunappimport.interfaces.dbcontext;

import java.util.List;
import speedrunappimport.model.entity.*;

public interface IGameDB extends IBaseDB<Game, Integer>
{
    public List<Game> findByCodeIn(List<String> codes);
}
