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

public class GameRepository extends BaseRepository implements IGameRepository {
	private IGameDB _gameDB;
	private IGameViewDB _gameViewDB;
	private ICategoryDB _categoryDB;
	private ILevelDB _levelDB;
	private IVariableDB _variableDB;
	private IVariableValueDB _variableValueDB;
	private IGamePlatformDB _gamePlatformDB;
	private Logger _logger;

	public GameRepository(IGameDB gameDB, IGameViewDB gameViewDB, ICategoryDB categoryDB, ILevelDB levelDB, IVariableDB variableDB, IVariableValueDB variableValueDB, IGamePlatformDB gamePlatformDB, Logger logger) {
		_gameDB = gameDB;
		_gameViewDB = gameViewDB;
		_categoryDB = categoryDB;
		_levelDB = levelDB;
		_variableDB = variableDB;
		_variableValueDB = variableValueDB;
		_gamePlatformDB = gamePlatformDB;
		_logger = logger;
	}

	public void SaveGames(List<Game> games) {
		_logger.info("Started SaveGames: {}", games.size());
	
		var count = 1;
		for (var game : games) {
			SaveGame(game);
			
			_logger.info("Saved games {}/ {}", count, games.size());
			count++;			
		}

		_logger.info("Completed SaveGames");
	}

	@Transactional(rollbackFor = { Exception.class })
	public void SaveGame(Game game) {
		_logger.info("Saving gameId: {}, code: {}", game.getId(), game.getCode());
	
		if (game.getId() != 0) {
			game.setModifiedDate(Instant.now());	
	
			_logger.info("Deleting secondary game entities");	
			if (!game.getCategoriesToRemove().isEmpty()) {
				_categoryDB.deleteAllById(game.getCategoriesToRemove());
			}
	
			if (!game.getLevelsToRemove().isEmpty()) {
				_levelDB.deleteAllById(game.getLevelsToRemove());
			}
	
			if (!game.getVariablesToRemove().isEmpty()) {
				_variableDB.deleteAllById(game.getVariablesToRemove());
			}
	
			if (!game.getVariableValuesToRemove().isEmpty()) {
				_variableValueDB.deleteAllById(game.getVariableValuesToRemove());
			}

			if (!game.getGamePlatforms().isEmpty()) {
				_gamePlatformDB.deleteAllById(game.getGamePlatformsToRemove());
			}				
		}

		_gameDB.save(game);

		game.getCategories().forEach(i -> i.setGameId(game.getId()));
		_categoryDB.saveAll(game.getCategories());

		game.getLevels().forEach(i -> i.setGameId(game.getId()));
		_levelDB.saveAll(game.getLevels());

		game.getVariables().forEach(i -> {
			i.setGameId(game.getId());
			i.setCategoryId(game.getCategories().stream().filter(g -> g.getCode() == i.getCategoryCode()).map(g -> g.getId()).findFirst().orElse(0));
			i.setLevelId(game.getLevels().stream().filter(g -> g.getCode() == i.getLevelCode()).map(g -> g.getId()).findFirst().orElse(0));
		});
		_variableDB.saveAll(game.getVariables());

		game.getVariableValues().forEach(i -> {
			i.setGameId(game.getId());
			i.setVariableId(game.getVariables().stream().filter(g -> g.getCode() == i.getVariableCode()).map(g -> g.getId()).findFirst().orElse(0));
		});

		_logger.info("Completed Saving gameId: {}, code: {}", game.getId(), game.getCode());
	}

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<Game> GetGamesByCode(List<String> codes) {
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<Game>();

		while (batchCount < codes.size()) {
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _gameDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}

		return results;
	}

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<GameView> GetGameViewsByCode(List<String> codes) {
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<GameView>();

		while (batchCount < codes.size()) {
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _gameViewDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}

		return results;
	}
}
