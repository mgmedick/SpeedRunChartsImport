package speedrunappimport.model.JSON;

import com.fasterxml.jackson.annotation.JsonRootName;

@JsonRootName("data")
public class GameNameResponse {
    public String international;
    public String japanese;
    public String twitch;

    public String getInternational() {
        return international;
    }

    public void setInternational(String international) {
        this.international = international;
    }    

    public String getJapanese() {
        return japanese;
    }

    public void setJapanese(String japanese) {
        this.japanese = japanese;
    }      

    public String getTwitch() {
        return twitch;
    }

    public void setTwitch(String twitch) {
        this.twitch = twitch;
    }        
}
