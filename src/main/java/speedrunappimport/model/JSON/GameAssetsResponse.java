package speedrunappimport.model.json;

import com.fasterxml.jackson.annotation.JsonProperty;

public record GameAssetsResponse(GameAssetResponse logo,
GameAssetResponse coverTiny,
GameAssetResponse coverSmall,
GameAssetResponse coverMedium,
GameAssetResponse coverLarge) {
}

/*
public class GameAssetsResponse 
{
    private GameAssetResponse logo;

    @JsonProperty("cover-tiny")
    private GameAssetResponse coverTiny;

    @JsonProperty("cover-small")
    private GameAssetResponse coverSmall;

    @JsonProperty("cover-medium")
    private GameAssetResponse coverMedium;
    
    @JsonProperty("cover-large")
    private GameAssetResponse coverLarge;  

    public GameAssetResponse getLogo() {
        return logo;
    }

    public void setLogo(GameAssetResponse logo) {
        this.logo = logo;
    }

    public GameAssetResponse getCoverTiny() {
        return coverTiny;
    }

    public void setCoverTiny(GameAssetResponse coverTiny) {
        this.coverTiny = coverTiny;
    }

    public GameAssetResponse getCoverSmall() {
        return coverSmall;
    }

    public void setCoverSmall(GameAssetResponse coverSmall) {
        this.coverSmall = coverSmall;
    }

    public GameAssetResponse getCoverMedium() {
        return coverMedium;
    }

    public void setCoverMedium(GameAssetResponse coverMedium) {
        this.coverMedium = coverMedium;
    }

    public GameAssetResponse getCoverLarge() {
        return coverLarge;
    }

    public void setCoverLarge(GameAssetResponse coverLarge) {
        this.coverLarge = coverLarge;
    }
}
*/
