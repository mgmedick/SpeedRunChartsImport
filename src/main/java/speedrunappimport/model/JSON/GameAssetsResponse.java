package speedrunappimport.model.json;

import com.fasterxml.jackson.annotation.JsonProperty;

public class GameAssetsResponse 
{
    public GameAssetResponse logo;

    @JsonProperty("cover-tiny")
    public GameAssetResponse coverTiny;

    @JsonProperty("cover-small")
    public GameAssetResponse coverSmall;

    @JsonProperty("cover-medium")
    public GameAssetResponse coverMedium;
    
    @JsonProperty("cover-large")
    public GameAssetResponse coverLarge;  
}
