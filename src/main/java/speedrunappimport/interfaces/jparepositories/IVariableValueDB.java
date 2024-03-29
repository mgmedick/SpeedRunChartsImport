package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface IVariableValueDB extends IBaseDB<VariableValue, Integer>
{
    @Query("SELECT va FROM tbl_variablevalue va WHERE va.code IN :codes AND va.deleted = false")    
    public List<VariableValue> findByCodeIn(List<String> codes);

    @Query("UPDATE tbl_variablevalue va SET va.deleted = true WHERE va.id IN :ids")
    @Modifying
    public void softDeleteAllById(List<Integer> ids);      
}
