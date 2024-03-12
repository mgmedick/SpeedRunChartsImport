package speedrunappimport.model.json;

import java.time.Instant;
import java.time.LocalDate;
import java.util.HashMap;

import com.fasterxml.jackson.annotation.JsonProperty;

public record GameResponse(String id,
GameNameResponse names,
String abbreviation,
String weblink,
Integer released,
LocalDate releaseDate,
GameRulesetResponse ruleset,
Boolean romhack,
String[] gametypes,
String[] platforms,
String[] genres,
String[] engines,
String[] developers,
String[] publishers,
HashMap<String, String> moderators,
Instant created,
GameAssetsResponse assets,
GameLevelResponse levels) {
}

/*
public class GameResponse
{
    private String id;
    private GameNameResponse names;
    private String abbreviation;
    private String weblink;
    private int released;

    @JsonProperty("release-date")
    private LocalDate releasedate;

    // private GameRulesetResponse ruleset;
    private boolean romhack;
    private String[] gametypes;
    private String[] platforms;
    private String[] genres;
    private String[] engines;  
    private String[] developers;      
    private String[] publishers;
    private HashMap<String, String> moderators;  
    private Instant created;
    // private GameAssetsResponse assets;
    // private GameLevelResponse levels;  

    public String getId() {
        return id;
    }
    public void setId(String id) {
        this.id = id;
    }
    public GameNameResponse getNames() {
        return names;
    }
    public void setNames(GameNameResponse names) {
        this.names = names;
    }
    public String getAbbreviation() {
        return abbreviation;
    }
    public void setAbbreviation(String abbreviation) {
        this.abbreviation = abbreviation;
    }
    public String getWeblink() {
        return weblink;
    }
    public void setWeblink(String weblink) {
        this.weblink = weblink;
    }
    public int getReleased() {
        return released;
    }
    public void setReleased(int released) {
        this.released = released;
    }
    public LocalDate getReleasedate() {
        return releasedate;
    }
    public void setReleasedate(LocalDate releasedate) {
        this.releasedate = releasedate;
    }
    // public GameRulesetResponse getRuleset() {
    //     return ruleset;
    // }
    // public void setRuleset(GameRulesetResponse ruleset) {
    //     this.ruleset = ruleset;
    // }
    public boolean isRomhack() {
        return romhack;
    }
    public void setRomhack(boolean romhack) {
        this.romhack = romhack;
    }
    public String[] getGametypes() {
        return gametypes;
    }
    public void setGametypes(String[] gametypes) {
        this.gametypes = gametypes;
    }
    public String[] getPlatforms() {
        return platforms;
    }
    public void setPlatforms(String[] platforms) {
        this.platforms = platforms;
    }
    public String[] getGenres() {
        return genres;
    }
    public void setGenres(String[] genres) {
        this.genres = genres;
    }
    public String[] getEngines() {
        return engines;
    }
    public void setEngines(String[] engines) {
        this.engines = engines;
    }
    public String[] getDevelopers() {
        return developers;
    }
    public void setDevelopers(String[] developers) {
        this.developers = developers;
    }
    public String[] getPublishers() {
        return publishers;
    }
    public void setPublishers(String[] publishers) {
        this.publishers = publishers;
    }
    public HashMap<String, String> getModerators() {
        return moderators;
    }
    public void setModerators(HashMap<String, String> moderators) {
        this.moderators = moderators;
    }
    public Instant getCreated() {
        return created;
    }
    public void setCreated(Instant created) {
        this.created = created;
    }
    // public GameAssetsResponse getAssets() {
    //     return assets;
    // }
    // public void setAssets(GameAssetsResponse assets) {
    //     this.assets = assets;
    // }
    // public GameLevelResponse getLevels() {
    //     return levels;
    // }
    // public void setLevels(GameLevelResponse levels) {
    //     this.levels = levels;
    // }   
}
*/

