package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface ILevelDB extends IBaseDB<Level, Integer> 
{
    @Query("SELECT l FROM tbl_level l WHERE l.code IN :codes AND l.deleted = false")    
    public List<Level> findByCodeIn(List<String> codes);

    @Query("UPDATE tbl_level l SET l.deleted = true WHERE l.id IN :ids")
    @Modifying
    public void softDeleteAllById(List<Integer> ids);        
}
