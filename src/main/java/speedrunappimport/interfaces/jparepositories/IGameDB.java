package speedrunappimport.interfaces.jparepositories;

import java.time.Instant;
import java.util.List;

import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import speedrunappimport.model.entity.*;

public interface IGameDB extends IBaseDB<Game, Integer>
{
    public List<Game> findByCodeIn(List<String> codes);

    @Query("SELECT g FROM tbl_game g WHERE COALESCE(g.modifiedDate, g.createdDate) > :compareDate")
    List<Game> findAllWithModifiedDateAfter(Instant compareDate); 
}
