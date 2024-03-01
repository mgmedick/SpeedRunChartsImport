package speedrunappimport.interfaces.repositories;

import org.springframework.data.repository.CrudRepository;
import speedrunappimport.model.Data.*;

public interface IGameCrudRepository extends CrudRepository<Game, Integer> 
{
}
