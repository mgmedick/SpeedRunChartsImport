package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface IPlayerDB extends IBaseDB<Player, Integer>
{
    public List<Player> findByCodeIn(List<String> codes);
}
