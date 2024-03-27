package speedrunappimport.interfaces.jparepositories;

import java.util.List;
import speedrunappimport.model.entity.*;

public interface IVariableDB extends IBaseDB<Variable, Integer>
{
    public List<Variable> findByCodeIn(List<String> codes);
}
