package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import org.hibernate.annotations.*;

@Entity
@Table(name = "tbl_game_platform")
@SQLDelete(sql = "UPDATE tbl_game_platform SET deleted = true WHERE id=?")
@SQLRestriction("deleted = false")
public class GamePlatform {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)    
    private int id;
    private int gameId;
    private int platformId;
    private boolean deleted;
 
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
    public boolean isDeleted() {
        return deleted;
    }
    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    } 
}


