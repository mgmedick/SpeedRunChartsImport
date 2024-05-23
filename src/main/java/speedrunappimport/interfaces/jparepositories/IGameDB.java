package speedrunappimport.interfaces.jparepositories;

import java.sql.Date;
import java.time.Instant;
import java.util.List;

import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import speedrunappimport.model.entity.*;

public interface IGameDB extends IBaseDB<Game, Integer>
{
    public List<Game> findByCodeIn(List<String> codes);

    //@Query("select g from tbl_game g where g.modifiedDate > :modifiedDate")
    //List<Game> findAllWithModifiedDateAfter(@Param("modifiedDate") Date modifiedDate); 
}
