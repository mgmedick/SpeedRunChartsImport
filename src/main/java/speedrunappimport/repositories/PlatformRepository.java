package speedrunappimport.repositories;

import java.util.List;

import org.slf4j.Logger;
import speedrunappimport.interfaces.jparepositories.*;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.model.entity.*;

public class PlatformRepository extends BaseRepository implements IPlatformRepository {
	private IPlatformDB _platformDB;
	private Logger _logger;

	public PlatformRepository(IPlatformDB platformDB, Logger logger) {
		_platformDB = platformDB;
		_logger = logger;
	}

	public void SavePlatforms(List<Platform> platforms) {	
		_platformDB.saveAll(platforms);
	}

	public List<Platform> GetAllPlatforms() {	
		return _platformDB.findAll();
	}	
}
