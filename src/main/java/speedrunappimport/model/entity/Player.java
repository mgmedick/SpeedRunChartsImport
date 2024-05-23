package speedrunappimport.model.entity;

import java.time.Instant;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

@Entity
@Table(name = "tbl_player")
public class Player {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String name;
    private String code;
    private int playerTypeId;
    @Transient
    private Instant createdDate;
    private Instant modifiedDate;

    @Transient
    private PlayerLink playerLink;

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getCode() {
        return code;
    }

    public void setCode(String code) {
        this.code = code;
    }
    
    public int getPlayerTypeId() {
        return playerTypeId;
    }

    public void setPlayerTypeId(int playerTypeId) {
        this.playerTypeId = playerTypeId;
    }

    public Instant getCreatedDate() {
        return createdDate;
    }

    public void setCreatedDate(Instant createdDate) {
        this.createdDate = createdDate;
    }

    public Instant getModifiedDate() {
        return modifiedDate;
    }

    public void setModifiedDate(Instant modifiedDate) {
        this.modifiedDate = modifiedDate;
    }

    public PlayerLink getPlayerLink() {
        return playerLink;
    }

    public void setPlayerLink(PlayerLink playerLink) {
        this.playerLink = playerLink;
    }
}
