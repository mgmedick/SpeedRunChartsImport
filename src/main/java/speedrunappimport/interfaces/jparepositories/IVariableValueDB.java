package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface IVariableValueDB extends IBaseDB<VariableValue, Integer>
{
    public List<VariableValue> findByCodeIn(List<String> codes);   
}
