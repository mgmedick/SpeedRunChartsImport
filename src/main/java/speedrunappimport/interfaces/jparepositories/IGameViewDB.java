package speedrunappimport.interfaces.jparepositories;

import java.util.List;
import speedrunappimport.model.entity.*;

public interface IGameViewDB extends IBaseDB<GameView, Integer>
{
    public List<GameView> findByCodeIn(List<String> codes);
}
