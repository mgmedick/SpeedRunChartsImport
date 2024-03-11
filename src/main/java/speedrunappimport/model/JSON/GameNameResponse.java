package speedrunappimport.model.json;

public record GameNameResponse(String international, String japanese, String twitch) {
}
/*
public class GameNameResponse 
{
    private String international;
    private String japanese;
    private String twitch;

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
*/
