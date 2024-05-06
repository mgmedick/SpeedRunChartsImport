package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import org.hibernate.annotations.*;

@Entity
@Table(name = "tbl_speedrun_player")
@SQLDelete(sql = "UPDATE tbl_speedrun_player SET deleted = true WHERE id=?")
@SQLRestriction("deleted = false")
public class SpeedRunPlayer {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private int speedRunId;
    private int playerId;
    private int playerTypeId;
    private boolean deleted;

    public int getId() {
        return id;
    }
    public void setId(int id) {
        this.id = id;
    }
    public int getSpeedRunId() {
        return speedRunId;
    }
    public void setSpeedRunId(int speedRunId) {
        this.speedRunId = speedRunId;
    }
    public int getPlayerId() {
        return playerId;
    }
    public void setPlayerId(int playerId) {
        this.playerId = playerId;
    }
    public int getPlayerTypeId() {
        return playerTypeId;
    }
    public void setPlayerTypeId(int playerTypeId) {
        this.playerTypeId = playerTypeId;
    }
    public boolean isDeleted() {
        return deleted;
    }
    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    }
}
