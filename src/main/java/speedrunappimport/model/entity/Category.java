package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

import org.hibernate.annotations.*;

@Entity
@Table(name = "tbl_category")
@SQLDelete(sql = "UPDATE tbl_category SET deleted = true WHERE id=?")
@SQLRestriction("deleted = false")
public class Category {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String name;
    private String code;
    private int gameId;
    private int categoryTypeId;
    private boolean isMiscellaneous;
    private boolean isTimerAscending;
    private boolean deleted;

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

    public boolean isMiscellaneous() {
        return isMiscellaneous;
    }

    public void setMiscellaneous(boolean isMiscellaneous) {
        this.isMiscellaneous = isMiscellaneous;
    }

    public boolean isIsTimerAscending() {
        return isTimerAscending;
    }

    public void setIsTimerAscending(boolean isTimerAscending) {
        this.isTimerAscending = isTimerAscending;
    }

    public boolean isDeleted() {
        return deleted;
    }

    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    }    
}
