package speedrunappimport.repositories;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;
import java.time.Instant;

import org.slf4j.Logger;
import org.springframework.transaction.annotation.Isolation;
import org.springframework.transaction.annotation.Transactional;

import speedrunappimport.interfaces.jparepositories.*;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.model.entity.*;

public class PlayerRepository extends BaseRepository implements IPlayerRepository {
	private IPlayerDB _playerDB;
	private IPlayerViewDB _playerViewDB;
	private IPlayerLinkDB _playerLinkDB;

	private Logger _logger;

	public PlayerRepository(IPlayerDB playerDB, IPlayerViewDB playerViewDB, IPlayerLinkDB playerLinkDB, Logger logger) {
		_playerDB = playerDB;
		_playerViewDB = playerViewDB;
		_playerLinkDB = playerLinkDB;
		_logger = logger;
	}
	
	public void SavePlayers(List<Player> players) {
		_logger.info("Started SavePlayers: {}", players.size());
	
		var count = 1;
		for (var player : players) {
			SavePlayer(player);
			
			_logger.info("Saved players {}/ {}", count, players.size());
			count++;			
		}

		_logger.info("Completed SavePlayers");
	}

	@Transactional(rollbackFor = { Exception.class })
	public void SavePlayer(Player player) {
		_logger.info("Saving playerId: {}, code: {}", player.getId(), player.getCode());
	
		if (player.getId() != 0) {
			player.setModifiedDate(Instant.now());
		}

		_playerDB.save(player);

		player.getPlayerLink().setPlayerId(player.getId());
		_playerLinkDB.save(player.getPlayerLink());

		_logger.info("Completed Saving playerId: {}, code: {}", player.getId(), player.getCode());
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
