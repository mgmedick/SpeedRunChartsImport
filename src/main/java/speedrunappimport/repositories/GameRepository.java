package speedrunappimport.repositories;

import java.util.List;
import java.util.ArrayList;
import java.util.stream.Collectors;

import org.springframework.transaction.annotation.Isolation;
import org.springframework.transaction.annotation.Transactional;
import speedrunappimport.interfaces.jparepositories.*;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.model.entity.*;

public class GameRepository extends BaseRepository implements IGameRepository
{
	private IGameDB _gameDB;
	private IGameViewDB _gameViewDB;
	private ILevelDB _levelDB;

	public GameRepository(IGameDB gameDB, IGameViewDB gameViewDB, ILevelDB levelDB) {
		_gameDB = gameDB;
		_gameViewDB = gameViewDB;
		_levelDB = levelDB;
	}

	@Transactional(rollbackFor = { Exception.class })
	public void SaveGames(List<Game> games, List<Level> levels)
	{
		for (Game game : games)
		{
			var gameLevels = levels.stream().filter(x -> x.getGameId() == game.getId()).collect(Collectors.toList());
			_levelDB.saveAll(gameLevels);
			_gameDB.save(game);
		}
	}

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<Game> GetGamesByCode(List<String> codes)
	{
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<Game>();

		while (batchCount < codes.size())
		{
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _gameDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}
		
		return results;
	}

	@Transactional(isolation = Isolation.READ_UNCOMMITTED)
	public List<GameView> GetGameViewsByCode(List<String> codes)
	{
		var maxBatchCount = super.maxQueryLimit;
		var batchCount = 0;
		var results = new ArrayList<GameView>();

		while (batchCount < codes.size())
		{
			var codesBatch = codes.stream().skip(batchCount).limit(maxBatchCount).collect(Collectors.toList());
			var resultsBatch = _gameViewDB.findByCodeIn(codesBatch);
			results.addAll(resultsBatch);
			batchCount += maxBatchCount;
		}
		
		return results;
	}	
}
