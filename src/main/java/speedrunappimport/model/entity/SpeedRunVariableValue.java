package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import org.hibernate.annotations.*;

@Entity
@Table(name = "tbl_speedrun_variablevalue")
@SQLDelete(sql = "UPDATE tbl_speedrun_variablevalue SET deleted = true WHERE id=?")
@SQLRestriction("deleted = false")
public class SpeedRunVariableValue {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private int speedRunId;
    private int VariableId;
    private int VariableValueId;
    private boolean deleted;

    @Transient
    private String variableCode;

    @Transient
    private String variableValueCode;

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
    public int getVariableId() {
        return VariableId;
    }
    public void setVariableId(int variableId) {
        VariableId = variableId;
    }
    public int getVariableValueId() {
        return VariableValueId;
    }
    public void setVariableValueId(int variableValueId) {
        VariableValueId = variableValueId;
    }
    public boolean isDeleted() {
        return deleted;
    }
    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    }
    public String getVariableCode() {
        return variableCode;
    }
    public void setVariableCode(String variableCode) {
        this.variableCode = variableCode;
    }
    public String getVariableValueCode() {
        return variableValueCode;
    }
    public void setVariableValueCode(String variableValueCode) {
        this.variableValueCode = variableValueCode;
    }   
}
