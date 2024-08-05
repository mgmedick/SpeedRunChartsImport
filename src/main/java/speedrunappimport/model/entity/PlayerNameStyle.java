package speedrunappimport.model.entity;

import java.io.Serializable;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "tbl_player_namestyle")
public class PlayerNameStyle implements Serializable {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private int playerId;
    private Boolean isGradient;
    private String colorLight;
    private String colorDark;
    private String colorToLight;
    private String colorToDark;

    public int getId() {
        return id;
    }
    public void setId(int id) {
        this.id = id;
    }
    public int getPlayerId() {
        return playerId;
    }
    public void setPlayerId(int playerId) {
        this.playerId = playerId;
    }
    public Boolean getIsGradient() {
        return isGradient;
    }
    public void setIsGradient(Boolean isGradient) {
        this.isGradient = isGradient;
    }
    public String getColorLight() {
        return colorLight;
    }
    public void setColorLight(String colorLight) {
        this.colorLight = colorLight;
    }
    public String getColorDark() {
        return colorDark;
    }
    public void setColorDark(String colorDark) {
        this.colorDark = colorDark;
    }
    public String getColorToLight() {
        return colorToLight;
    }
    public void setColorToLight(String colorToLight) {
        this.colorToLight = colorToLight;
    }
    public String getColorToDark() {
        return colorToDark;
    }
    public void setColorToDark(String colorToDark) {
        this.colorToDark = colorToDark;
    }
}
