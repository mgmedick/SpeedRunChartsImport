package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface IGameCategoryTypeDB extends IBaseDB<GameCategoryType, Integer>
{
    @Query("UPDATE tbl_game_categorytype gc SET gc.deleted = true WHERE gc.id IN :ids")
    @Modifying
    public void softDeleteAllById(List<Integer> ids);       
}
