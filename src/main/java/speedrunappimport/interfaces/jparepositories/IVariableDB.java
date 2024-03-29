package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface IVariableDB extends IBaseDB<Variable, Integer>
{
    @Query("SELECT v FROM tbl_variable v WHERE v.code IN :codes AND v.deleted = false")    
    public List<Variable> findByCodeIn(List<String> codes);

    @Query("UPDATE tbl_variable v SET v.deleted = true WHERE v.id IN :ids")
    @Modifying
    public void softDeleteAllById(List<Integer> ids);         
}
