package speedrunappimport.model.JSON;

import java.time.Instant;
import java.time.LocalDate;
import java.util.HashMap;

import com.fasterxml.jackson.annotation.JsonProperty;

public class GameResponse
{
    public String id;
    public GameNameResponse names;
    public String abbreviation;
    public String weblink;
    public int released;

    @JsonProperty("release-date")
    public LocalDate releasedDate;

    public GameRulesetResponse ruleset;
    public boolean romhack;
    public String[] gametypes;
    public String[] platforms;
    public String[] genres;
    public String[] engines;  
    public String[] developers;      
    public String[] publishers;   
    public HashMap<String, String> moderators;  
    public Instant created;
    public GameAssetsResponse assets;
}
