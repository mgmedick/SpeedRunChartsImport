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
	private IGameLinkDB _gameLinkDB;
	private ICategoryDB _categoryDB;
	private ILevelDB _levelDB;
	private IVariableDB _variableDB;
	private IVariableValueDB _variableValueDB;
	private IGamePlatformDB _gamePlatformDB;
	private ICategoryTypeDB _categoryTypeDB;
	private IGameCategoryTypeDB _gameCategoryTypeDB;
	private Logger _logger;

	public GameRepository(IGameDB gameDB, IGameViewDB gameViewDB, IGameLinkDB gameLinkDB, ICategoryDB categoryDB, ILevelDB levelDB, IVariableDB variableDB, IVariableValueDB variableValueDB, IGamePlatformDB gamePlatformDB, ICategoryTypeDB categoryTypeDB, IGameCategoryTypeDB gameCategoryTypeDB, Logger logger) {
		_gameDB = gameDB;
		_gameViewDB = gameViewDB;
		_gameLinkDB = gameLinkDB;
		_categoryDB = categoryDB;
		_levelDB = levelDB;
		_variableDB = variableDB;
		_variableValueDB = variableValueDB;
		_gamePlatformDB = gamePlatformDB;
		_categoryTypeDB = categoryTypeDB;
		_gameCategoryTypeDB = gameCategoryTypeDB;
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
			_gameCategoryTypeDB.deleteAllById(game.getGameCategoryTypesToRemove());	
			_categoryDB.deleteAllById(game.getCategoriesToRemove());
			_levelDB.deleteAllById(game.getLevelsToRemove());
			_variableDB.deleteAllById(game.getVariablesToRemove());
			_variableValueDB.deleteAllById(game.getVariableValuesToRemove());
			_gamePlatformDB.deleteAllById(game.getGamePlatformsToRemove());			
		}

		_gameDB.save(game);

		game.getGameLink().setGameId(game.getId());
		_gameLinkDB.save(game.getGameLink());

		game.getGameCategoryTypes().forEach(i -> i.setGameId(game.getId()));
		_gameCategoryTypeDB.saveAll(game.getGameCategoryTypes());
		
		game.getCategories().forEach(i -> i.setGameId(game.getId()));
		_categoryDB.saveAll(game.getCategories());

		game.getLevels().forEach(i -> i.setGameId(game.getId()));
		_levelDB.saveAll(game.getLevels());

		game.getVariables().forEach(i -> {
			i.setGameId(game.getId());
			i.setCategoryId(game.getCategories().stream().filter(g -> g.getCode().equals(i.getCategoryCode())).map(g -> g.getId()).findFirst().orElse(null));
			i.setLevelId(game.getLevels().stream().filter(g -> g.getCode().equals(i.getLevelCode())).map(g -> g.getId()).findFirst().orElse(null));
		});
		_variableDB.saveAll(game.getVariables());

		game.getVariableValues().forEach(i -> {
			i.setGameId(game.getId());
			i.setVariableId(game.getVariables().stream().filter(g -> g.getCode().equals(i.getVariableCode())).map(g -> g.getId()).findFirst().orElse(null));
		});
		_variableValueDB.saveAll(game.getVariableValues());

		game.getGamePlatforms().forEach(i -> i.setGameId(game.getId()));
		_gamePlatformDB.saveAll(game.getGamePlatforms());

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

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<CategoryType> GetCategoryTypes() {
		return _categoryTypeDB.findAll();
	}	

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<Game> GetGamesModifiedAfter(Instant date) {
		var results = _gameDB.findAllWithModifiedDateAfter(date);
		
		return results;
	}	
}
