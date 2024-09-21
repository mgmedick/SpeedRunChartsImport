package speedrunappimport.model.entity;

import java.time.Instant;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.core.json.JsonReadFeature;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.MapperFeature;
import com.fasterxml.jackson.databind.json.JsonMapper;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

@Entity(name = "vw_speedrunsummary")
@Table(name = "vw_speedrunsummary")
public class SpeedRunSummaryView {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int sortOrder;
    private int id;
    private String code;  
    private int gameId;
    private String gameName;
    private String gameAbbr;
    private boolean showMilliseconds;
    private String gameCoverImageUrl;
    private int categoryTypeId;
    private String categoryTypeName;
    private int categoryId;
    private String categoryName;
    private Integer levelId;
    private String levelName;
    private String subCategoryVariableValueIds;
    private Integer rank;
    private long primaryTime;
    private Instant verifyDate;  
    private Instant createdDate;   
    private Instant modifiedDate;   
    private String subCategoryVariableValueNamesJson;    
    private String playersJson;   
    private String videosJson;    

    @Transient
    private ObjectMapper _mapper;

    public SpeedRunSummaryView() {
        _mapper = JsonMapper.builder()
                            .enable(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES)
                            .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
                            .configure(JsonReadFeature.ALLOW_UNESCAPED_CONTROL_CHARS.mappedFeature(), true)
                            .addModule(new JavaTimeModule())
                            .build();
    }
        
    public int getSortOrder() {
        return sortOrder;
    }

    public void setSortOrder(int sortOrder) {
        this.sortOrder = sortOrder;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getCode() {
        return code;
    }

    public void setCode(String code) {
        this.code = code;
    }

    public int getGameId() {
        return gameId;
    }

    public void setGameId(int gameId) {
        this.gameId = gameId;
    }

    public String getGameName() {
        return gameName;
    }

    public void setGameName(String gameName) {
        this.gameName = gameName;
    }

    public String getGameAbbr() {
        return gameAbbr;
    }

    public void setGameAbbr(String gameAbbr) {
        this.gameAbbr = gameAbbr;
    }

    public boolean isShowMilliseconds() {
        return showMilliseconds;
    }

    public void setShowMilliseconds(boolean showMilliseconds) {
        this.showMilliseconds = showMilliseconds;
    }

    public String getGameCoverImageUrl() {
        return gameCoverImageUrl;
    }

    public void setGameCoverImageUrl(String gameCoverImageUrl) {
        this.gameCoverImageUrl = gameCoverImageUrl;
    }

    public int getCategoryTypeId() {
        return categoryTypeId;
    }

    public void setCategoryTypeId(int categoryTypeId) {
        this.categoryTypeId = categoryTypeId;
    }

    public String getCategoryTypeName() {
        return categoryTypeName;
    }

    public void setCategoryTypeName(String categoryTypeName) {
        this.categoryTypeName = categoryTypeName;
    }

    public int getCategoryId() {
        return categoryId;
    }

    public void setCategoryId(int categoryId) {
        this.categoryId = categoryId;
    }

    public String getCategoryName() {
        return categoryName;
    }

    public void setCategoryName(String categoryName) {
        this.categoryName = categoryName;
    }

    public Integer getLevelId() {
        return levelId;
    }

    public void setLevelId(Integer levelId) {
        this.levelId = levelId;
    }

    public String getLevelName() {
        return levelName;
    }

    public void setLevelName(String levelName) {
        this.levelName = levelName;
    }

    public String getSubCategoryVariableValueIds() {
        return subCategoryVariableValueIds;
    }

    public void setSubCategoryVariableValueIds(String subCategoryVariableValueIds) {
        this.subCategoryVariableValueIds = subCategoryVariableValueIds;
    }

    public Integer getRank() {
        return rank;
    }

    public void setRank(Integer rank) {
        this.rank = rank;
    }

    public long getPrimaryTime() {
        return primaryTime;
    }

    public void setPrimaryTime(long primaryTime) {
        this.primaryTime = primaryTime;
    }

    public Instant getVerifyDate() {
        return verifyDate;
    }

    public void setVerifyDate(Instant verifyDate) {
        this.verifyDate = verifyDate;
    }

    public Instant getCreatedDate() {
        return createdDate;
    }

    public void setCreatedDate(Instant createdDate) {
        this.createdDate = createdDate;
    }
    
    public Instant getModifiedDate() {
        return modifiedDate;
    }

    public void setModifiedDate(Instant modifiedDate) {
        this.modifiedDate = modifiedDate;
    }

    public String getSubCategoryVariableValueNamesJson() {
        return subCategoryVariableValueNamesJson;
    }

    public void setSubCategoryVariableValueNamesJson(String subCategoryVariableValueNamesJson) {
        this.subCategoryVariableValueNamesJson = subCategoryVariableValueNamesJson;
    }

    public String getPlayersJson() {
        return playersJson;
    }

    public void setPlayersJson(String playersJson) {
        this.playersJson = playersJson;
    }

    public String getVideosJson() {
        return videosJson;
    }

    public void setVideosJson(String videosJson) {
        this.videosJson = videosJson;
    }
    
    public List<SpeedRunPlayer> getPlayers() {
        List<SpeedRunPlayer> results = new ArrayList<SpeedRunPlayer>();
        
        try {
            if (this.playersJson != null) {
                results = Arrays.asList(_mapper.readValue(this.playersJson, SpeedRunPlayer[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
        
        return results;
    }       
    public List<String> getSubCategoryVariableValueNames() {
        List<String> results = new ArrayList<String>();
        
        try {
            if (this.subCategoryVariableValueNamesJson != null) {
                results = Arrays.asList(_mapper.readValue(this.subCategoryVariableValueNamesJson, String[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
        
        return results;
    }    
    public List<SpeedRunVideo> getVideos() {
        List<SpeedRunVideo> results = new ArrayList<SpeedRunVideo>();
        
        try {
            if (this.videosJson != null) {
                results = Arrays.asList(_mapper.readValue(this.videosJson, SpeedRunVideo[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
        
        return results;
    }    
}
