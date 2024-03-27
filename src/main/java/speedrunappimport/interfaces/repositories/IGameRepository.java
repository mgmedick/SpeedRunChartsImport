package speedrunappimport.interfaces.repositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface IGameRepository {
    void SaveGames(List<Game> games);
    List<Game> GetGamesByCode(List<String> codes);
    List<GameView> GetGameViewsByCode(List<String> codes);
}


