package speedrunappimport.model.entity;

import java.time.Instant;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Map;
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

@Entity
@Table(name = "vw_speedrun")
public class SpeedRunView {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String code;  
    private int gameId;
    private int categoryTypeId;
    private int categoryId;
    private Integer levelId;
    private String subCategoryVariableValueIds;
    private Integer platformId;
    private Integer rank;
    private long primaryTime;
    private Instant dateSubmitted;
    private Instant verifyDate;   
    private int speedRunLinkId;
    private String srcUrl;
    private String playersJson;   
    private String variableValuesJson;    
    private String videosJson;    

    @Transient
    private ObjectMapper _mapper;

    public SpeedRunView() {
        _mapper = JsonMapper.builder()
                            .enable(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES)
                            .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
                            .configure(JsonReadFeature.ALLOW_UNESCAPED_CONTROL_CHARS.mappedFeature(), true)
                            .addModule(new JavaTimeModule())
                            .build();
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
    public int getCategoryTypeId() {
        return categoryTypeId;
    }
    public void setCategoryTypeId(int categoryTypeId) {
        this.categoryTypeId = categoryTypeId;
    }    
    public int getCategoryId() {
        return categoryId;
    }
    public void setCategoryId(int categoryId) {
        this.categoryId = categoryId;
    }
    public Integer getLevelId() {
        return levelId;
    }
    public void setLevelId(Integer levelId) {
        this.levelId = levelId;
    }
    public String getSubCategoryVariableValueIds() {
        return subCategoryVariableValueIds;
    }
    public void setSubCategoryVariableValueIds(String subCategoryVariableValueIds) {
        this.subCategoryVariableValueIds = subCategoryVariableValueIds;
    }        
    public Integer getPlatformId() {
        return platformId;
    }
    public void setPlatformId(Integer platformId) {
        this.platformId = platformId;
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
    public Instant getDateSubmitted() {
        return dateSubmitted;
    }
    public void setDateSubmitted(Instant dateSubmitted) {
        this.dateSubmitted = dateSubmitted;
    }
    public Instant getVerifyDate() {
        return verifyDate;
    }
    public void setVerifyDate(Instant verifyDate) {
        this.verifyDate = verifyDate;
    }      
    public int getSpeedRunLinkId() {
        return speedRunLinkId;
    }
    public void setSpeedRunLinkId(int speedRunLinkId) {
        this.speedRunLinkId = speedRunLinkId;
    }
    public String getSrcUrl() {
        return srcUrl;
    }
    public void setSrcUrl(String srcUrl) {
        this.srcUrl = srcUrl;
    }
    public String getPlayersJson() {
        return playersJson;
    }
    public void setPlayersJson(String playersJson) {
        this.playersJson = playersJson;
    }    
    public String getVariableValuesJson() {
        return variableValuesJson;
    }
    public void setVariableValuesJson(String variableValuesJson) {
        this.variableValuesJson = variableValuesJson;
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
    public List<SpeedRunVariableValue> getVariableValues() {
        List<SpeedRunVariableValue> results = new ArrayList<SpeedRunVariableValue>();
        
        try {
            if (this.variableValuesJson != null) {
                results = Arrays.asList(_mapper.readValue(this.variableValuesJson, SpeedRunVariableValue[].class));
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
