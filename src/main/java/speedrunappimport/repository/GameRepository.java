package speedrunappimport.repository;

import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.StreamSupport;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.model.Data.Game;

public class GameRepository implements IGameRepository
{
	private IGameCrudRepository _db;

	public GameRepository(IGameCrudRepository db) {
		_db = db;
	}

	public List<Game> GetAllGames()
	{
		var games = _db.findAll();
		
		return StreamSupport.stream(games.spliterator(), false)
                            .collect(Collectors.toList());
	}
}
