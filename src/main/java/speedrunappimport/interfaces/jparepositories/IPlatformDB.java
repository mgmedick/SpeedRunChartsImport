package speedrunappimport.interfaces.jparepositories;

import java.util.List;

import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface IPlatformDB extends IBaseDB<Platform, Integer>
{
    @Query("SELECT p FROM tbl_platform p WHERE p.code IN :codes AND p.deleted = false")    
    public List<Platform> findByCodeIn(List<String> codes);
}
