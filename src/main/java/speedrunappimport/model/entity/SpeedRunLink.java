package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "tbl_speedrun_link")
public class SpeedRunLink {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private int speedRunId;
    private String speedRunComUrl;

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
    public String getSpeedRunComUrl() {
        return speedRunComUrl;
    }
    public void setSpeedRunComUrl(String speedRunComUrl) {
        this.speedRunComUrl = speedRunComUrl;
    }
}
