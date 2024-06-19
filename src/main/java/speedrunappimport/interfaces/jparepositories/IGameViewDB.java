package speedrunappimport.interfaces.jparepositories;

import java.time.Instant;
import java.util.List;

import org.springframework.data.jpa.repository.Query;
import speedrunappimport.model.entity.*;

public interface IGameViewDB extends IBaseDB<GameView, Integer>
{
    public List<GameView> findByCodeIn(List<String> codes);

    @Query("SELECT g FROM vw_game g WHERE COALESCE(g.modifiedDate, g.createdDate) > :compareDate")
    List<GameView> findAllWithModifiedDateAfter(Instant compareDate);     
}
