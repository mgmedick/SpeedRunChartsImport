package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "tbl_game_platform")
public class GamePlatform {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)    
    private int id;
    private int gameId;
    private int platformId;

    public int getId() {
        return id;
    }
    public void setId(int id) {
        this.id = id;
    }
    public int getGameId() {
        return gameId;
    }
    public void setGameId(int gameId) {
        this.gameId = gameId;
    }
    public int getPlatformId() {
        return platformId;
    }
    public void setPlatformId(int platformId) {
        this.platformId = platformId;
    }
}


