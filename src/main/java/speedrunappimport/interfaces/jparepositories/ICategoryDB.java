package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface ICategoryDB extends IBaseDB<Category, Integer>
{
    @Query("SELECT c FROM tbl_category c WHERE c.code IN :codes AND c.deleted = false")    
    public List<Category> findByCodeIn(List<String> codes);

    @Query("UPDATE tbl_category c SET c.deleted = true WHERE c.id IN :ids")
    @Modifying
    public void softDeleteAllById(List<Integer> ids);     
}
