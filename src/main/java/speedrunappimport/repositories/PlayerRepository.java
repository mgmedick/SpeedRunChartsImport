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

public class PlayerRepository extends BaseRepository implements IPlayerRepository {
	private IPlayerDB _playerDB;
	private IPlayerViewDB _playerViewDB;

	private Logger _logger;

	public PlayerRepository(IPlayerDB playerDB, IPlayerViewDB playerViewDB, Logger logger) {
		_playerDB = playerDB;
		_playerViewDB = playerViewDB;
		_logger = logger;
	}

	public void SavePlayers(List<Player> players) {	
		_playerDB.saveAll(players);
	}

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<Player> GetPlayersByCode(List<String> codes) {
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<Player>();

		while (batchCount < codes.size()) {
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _playerDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}

		return results;
	}

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<PlayerView> GetPlayerViewsByCode(List<String> codes) {
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<PlayerView>();

		while (batchCount < codes.size()) {
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _playerViewDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}

		return results;
	}	
}
