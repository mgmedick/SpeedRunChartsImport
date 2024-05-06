package speedrunappimport.repositories;

import java.util.List;
import java.time.Instant;
import java.util.ArrayList;
import java.util.stream.Collectors;

import org.slf4j.Logger;
import org.springframework.transaction.annotation.Isolation;
import org.springframework.transaction.annotation.Transactional;
import speedrunappimport.interfaces.jparepositories.*;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.model.entity.*;

public class SpeedRunRepository extends BaseRepository implements ISpeedRunRepository {
	private ISpeedRunDB _speedRunDB;
	private ISpeedRunViewDB _speedRunViewDB;
	private ISpeedRunLinkDB _speedRunLinkDB;
	private ISpeedRunVariableValueDB _speedRunVariableValueDB;
	private ISpeedRunPlayerDB _speedRunPlayerDB;
	private ISpeedRunVideoDB _speedRunVideoDB;
	private Logger _logger;

	public SpeedRunRepository(ISpeedRunDB speedRunDB, ISpeedRunViewDB speedRunViewDB, ISpeedRunLinkDB speedRunLinkDB, ISpeedRunVariableValueDB speedRunVariableValueDB, ISpeedRunPlayerDB speedRunPlayerDB, ISpeedRunVideoDB speedRunVideoDB, Logger logger) {
		_speedRunDB = speedRunDB;
		_speedRunViewDB = speedRunViewDB;
		_speedRunLinkDB = speedRunLinkDB;
		_speedRunVariableValueDB = speedRunVariableValueDB;
		_speedRunPlayerDB = speedRunPlayerDB;
		_speedRunVideoDB = speedRunVideoDB;
		_logger = logger;
	}

	public void SaveSpeedRuns(List<SpeedRun> runs) {
		_logger.info("Started SaveSpeedRuns: {}", runs.size());
	
		var count = 1;
		for (var run : runs) {
			SaveSpeedRun(run);
			
			_logger.info("Saved runs {}/ {}", count, runs.size());
			count++;			
		}

		_logger.info("Completed SaveSpeedRuns");
	}

	@Transactional(rollbackFor = { Exception.class })
	public void SaveSpeedRun(SpeedRun run) {
		_logger.info("Saving runId: {}, code: {}", run.getId(), run.getCode());
	
		if (run.getId() != 0) {
			run.setModifiedDate(Instant.now());	
	
			_logger.info("Deleting secondary run entities");
			// _gameCategoryTypeDB.deleteAllById(game.getGameCategoryTypesToRemove());	
			// _categoryDB.deleteAllById(game.getCategoriesToRemove());
			// _levelDB.deleteAllById(game.getLevelsToRemove());
			// _variableDB.deleteAllById(game.getVariablesToRemove());
			// _variableValueDB.deleteAllById(game.getVariableValuesToRemove());
			// _gamePlatformDB.deleteAllById(game.getGamePlatformsToRemove());			
		}

		_speedRunDB.save(run);

		// game.getGameLink().setGameId(game.getId());
		// _gameLinkDB.save(game.getGameLink());

		// game.getGameCategoryTypes().forEach(i -> i.setGameId(game.getId()));
		// _gameCategoryTypeDB.saveAll(game.getGameCategoryTypes());
		
		// game.getCategories().forEach(i -> i.setGameId(game.getId()));
		// _categoryDB.saveAll(game.getCategories());

		// game.getLevels().forEach(i -> i.setGameId(game.getId()));
		// _levelDB.saveAll(game.getLevels());

		// game.getVariables().forEach(i -> {
		// 	i.setGameId(game.getId());
		// 	i.setCategoryId(game.getCategories().stream().filter(g -> g.getCode().equals(i.getCategoryCode())).map(g -> g.getId()).findFirst().orElse(null));
		// 	i.setLevelId(game.getLevels().stream().filter(g -> g.getCode().equals(i.getLevelCode())).map(g -> g.getId()).findFirst().orElse(null));
		// });
		// _variableDB.saveAll(game.getVariables());

		// game.getVariableValues().forEach(i -> {
		// 	i.setGameId(game.getId());
		// 	i.setVariableId(game.getVariables().stream().filter(g -> g.getCode().equals(i.getVariableCode())).map(g -> g.getId()).findFirst().orElse(null));
		// });
		// _variableValueDB.saveAll(game.getVariableValues());

		// game.getGamePlatforms().forEach(i -> i.setGameId(game.getId()));
		// _gamePlatformDB.saveAll(game.getGamePlatforms());

		// _logger.info("Completed Saving gameId: {}, code: {}", game.getId(), game.getCode());
	}

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<SpeedRun> GetSpeedRunsByCode(List<String> codes) {
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<SpeedRun>();

		while (batchCount < codes.size()) {
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _speedRunDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}

		return results;
	}

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<SpeedRunView> GetSpeedRunViewsByCode(List<String> codes) {
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<SpeedRunView>();

		while (batchCount < codes.size()) {
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _speedRunViewDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}

		return results;
	}	
}
