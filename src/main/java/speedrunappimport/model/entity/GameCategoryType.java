package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import java.util.List;

import org.hibernate.annotations.*;

@Entity
@Table(name = "tbl_game_categorytype")
@SQLDelete(sql = "UPDATE tbl_game_categorytype SET deleted = true WHERE id=?")
@SQLRestriction("deleted = false")
public class GameCategoryType {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)    
    private int id;
    private int gameId;
    private int categoryTypeId;
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
    public int getCategoryTypeId() {
        return categoryTypeId;
    }
    public void setCategoryTypeId(int categoryTypeId) {
        this.categoryTypeId = categoryTypeId;
    }
    public boolean isDeleted() {
        return deleted;
    }
    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    }    
}


