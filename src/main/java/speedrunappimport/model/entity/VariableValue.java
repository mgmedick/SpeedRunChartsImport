package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

@Entity
@Table(name = "tbl_variablevalue")
public class VariableValue {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String name;
    private String code;
    private int gameId;
    private int variableId;
    private boolean isMiscellaneous;
    private boolean deleted;

    @Transient
    private String variableCode;

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

    public int getVariableId() {
        return variableId;
    }

    public void setVariableId(int variableId) {
        this.variableId = variableId;
    }

    public boolean isIsMiscellaneous() {
        return isMiscellaneous;
    }

    public void setIsMiscellaneous(boolean isMiscellaneous) {
        this.isMiscellaneous = isMiscellaneous;
    }
    
    public String getVariableCode() {
        return variableCode;
    }

    public void setVariableCode(String variableCode) {
        this.variableCode = variableCode;
    }
    
    public boolean isDeleted() {
        return deleted;
    }

    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    }     
}
