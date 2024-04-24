package speedrunappimport.interfaces.jparepositories;

import java.time.Instant;
import java.util.List;

import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface IGameDB extends IBaseDB<Game, Integer>
{
    public List<Game> findByCodeIn(List<String> codes);

    @Query("select g from tbl_game g where g.modifieddate > :compareDate")
    List<Game> findAllWithModifiedDateAfter(Instant compareDate);    
}
