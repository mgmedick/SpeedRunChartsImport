package speedrunappimport.interfaces.repositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface IPlayerRepository {
    void SavePlayers(List<Player> players);
    List<Player> GetPlayersByCode(List<String> codes);
    List<PlayerView> GetPlayerViewsByCode(List<String> codes);
}


