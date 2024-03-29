package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface IGameDB extends IBaseDB<Game, Integer>
{
    @Query("SELECT g FROM tbl_game g WHERE g.code IN :codes AND g.deleted = false")    
    public List<Game> findByCodeIn(List<String> codes);
}
