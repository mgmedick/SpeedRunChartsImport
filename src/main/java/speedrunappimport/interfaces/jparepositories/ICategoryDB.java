package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface ICategoryDB extends IBaseDB<Category, Integer>
{
    public List<Category> findByCodeIn(List<String> codes);    
}
