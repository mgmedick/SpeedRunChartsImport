package speedrunappimport.interfaces.jparepositories;

import java.io.*;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.repository.NoRepositoryBean;

@NoRepositoryBean
public interface IBaseDB <T, ID extends Serializable> extends JpaRepository<T, ID>
{
}
