package speedrunappimport.repositories;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

import org.slf4j.Logger;
import org.springframework.transaction.annotation.Isolation;
import org.springframework.transaction.annotation.Transactional;

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
	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<Platform> GetPlatformsByCode(List<String> codes) {
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<Platform>();

		while (batchCount < codes.size()) {
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _platformDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}

		return results;
	}
}
