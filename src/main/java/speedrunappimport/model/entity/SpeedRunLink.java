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
    private String srcUrl;

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
    public String getSrcUrl() {
        return srcUrl;
    }
    public void setSrcUrl(String srcUrl) {
        this.srcUrl = srcUrl;
    }
}
