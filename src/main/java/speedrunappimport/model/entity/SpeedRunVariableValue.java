package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

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
}
