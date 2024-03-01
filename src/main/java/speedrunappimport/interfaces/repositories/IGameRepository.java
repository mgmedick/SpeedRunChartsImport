package speedrunappimport.interfaces.repositories;

import java.util.List;

import speedrunappimport.model.Data.*;

public interface IGameRepository {
    List<Game> GetAllGames();
}


