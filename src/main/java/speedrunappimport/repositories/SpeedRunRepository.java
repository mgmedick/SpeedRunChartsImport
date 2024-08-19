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
	private ISpeedRunPlayerDB _speedRunPlayerDB;
	private ISpeedRunVariableValueDB _speedRunVariableValueDB;
	private ISpeedRunVideoDB _speedRunVideoDB;
	private Logger _logger;

	public SpeedRunRepository(ISpeedRunDB speedRunDB, ISpeedRunViewDB speedRunViewDB, ISpeedRunLinkDB speedRunLinkDB, ISpeedRunPlayerDB speedRunPlayerDB, ISpeedRunVariableValueDB speedRunVariableValueDB, ISpeedRunVideoDB speedRunVideoDB, Logger logger) {
		_speedRunDB = speedRunDB;
		_speedRunViewDB = speedRunViewDB;
		_speedRunLinkDB = speedRunLinkDB;
		_speedRunPlayerDB = speedRunPlayerDB;	
		_speedRunVariableValueDB = speedRunVariableValueDB;
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
		// _logger.info("Saving runId: {}, code: {}", run.getId(), run.getCode());
	
		if (run.getId() != 0) {
			run.setModifiedDate(Instant.now());	
	
			// _logger.info("Deleting secondary run entities");
			_speedRunPlayerDB.deleteAllById(run.getPlayersToRemove());	
			_speedRunVariableValueDB.deleteAllById(run.getVariableValuesToRemove());	
			_speedRunVideoDB.deleteAllById(run.getVideosToRemove());	
		}

		_speedRunDB.save(run);

		run.getSpeedRunLink().setSpeedRunId(run.getId());
		_speedRunLinkDB.save(run.getSpeedRunLink());

		run.getPlayers().forEach(i -> i.setSpeedRunId(run.getId()));
		_speedRunPlayerDB.saveAll(run.getPlayers());
		
		run.getVariableValues().forEach(i -> i.setSpeedRunId(run.getId()));
		_speedRunVariableValueDB.saveAll(run.getVariableValues());

		run.getVideos().forEach(i -> i.setSpeedRunId(run.getId()));
		_speedRunVideoDB.saveAll(run.getVideos());

		// _logger.info("Completed Saving runId: {}, code: {}", run.getId(), run.getCode());
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

	public void DeleteObsoleteSpeedRuns(Instant lastImportDateUtc) {
		_logger.info("Started DeleteObsoleteSpeedRuns: {}", lastImportDateUtc);
	
		_speedRunDB.deleteObsoleteSpeedRuns(lastImportDateUtc);

		_logger.info("Completed DeleteObsoleteSpeedRuns");
	}	

	public void UpdateSpeedRunRanks(Instant lastImportDateUtc) {
		_logger.info("Started UpdateSpeedRunRanks: {}", lastImportDateUtc);
	
		_speedRunDB.updateSpeedRunRanks(lastImportDateUtc);

		_logger.info("Completed UpdateSpeedRunRanks");
	}	

	public void UpdateSpeedRunOrdered(Instant lastImportDateUtc) {
		_logger.info("Started UpdateSpeedRunOrdered: {}", lastImportDateUtc);
	
		_speedRunDB.updateSpeedRunOrdered(lastImportDateUtc);

		_logger.info("Completed UpdateSpeedRunOrdered");
	}		
}
