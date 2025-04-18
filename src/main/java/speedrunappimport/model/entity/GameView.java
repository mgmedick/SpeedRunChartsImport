package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import java.time.Instant;
import java.time.LocalDate;
import java.util.Arrays;
import java.util.List;
import java.util.ArrayList;

import com.fasterxml.jackson.databind.MapperFeature;
import com.fasterxml.jackson.core.json.JsonReadFeature;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.json.JsonMapper;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

@Entity(name = "vw_game")
@Table(name = "vw_game")
public class GameView {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String name;
    private String code;
    private String abbr;
    private boolean showMilliseconds;
    private LocalDate releaseDate;
    private Instant createdDate;
    private Instant modifiedDate;   
    private int gameLinkId;
    private String coverImageUrl;
    private String srcUrl;   
    private String gameCategoryTypesJson;
    private String categoriesJson;
    private String levelsJson;
    private String variablesJson;
    private String variableValuesJson;
    private String gamePlatformsJson;

    @Transient
    private ObjectMapper _mapper;

    public GameView() {
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

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getCode() {
        return code;
    }

    public void setCode(String code) {
        this.code = code;
    }   

    public String getAbbr() {
        return abbr;
    }

    public void setAbbr(String abbr) {
        this.abbr = abbr;
    }

    public Boolean getShowMilliseconds() {
        return showMilliseconds;
    }

    public void setShowMilliseconds(Boolean showMilliseconds) {
        this.showMilliseconds = showMilliseconds;
    }

    public LocalDate getReleaseDate() {
        return releaseDate;
    }

    public void setReleaseDate(LocalDate releaseDate) {
        this.releaseDate = releaseDate;
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

    public int getGameLinkId() {
        return gameLinkId;
    }

    public void setGameLinkId(int gameLinkId) {
        this.gameLinkId = gameLinkId;
    }

    public String getCoverImageUrl() {
        return coverImageUrl;
    }

    public void setCoverImageUrl(String coverImageUrl) {
        this.coverImageUrl = coverImageUrl;
    }

    public String getSrcUrl() {
        return srcUrl;
    }

    public void setSrcUrl(String srcUrl) {
        this.srcUrl = srcUrl;
    }

    public String getGameCategoryTypesJson() {
        return gameCategoryTypesJson;
    }

    public void setGameCategoryTypesJson(String gameCategoryTypesJson) {
        this.gameCategoryTypesJson = gameCategoryTypesJson;
    }

    public String getCategoriesJson() {
        return categoriesJson;
    }

    public void setCategoriesJson(String categoriesJson) {
        this.categoriesJson = categoriesJson;
    }

    public String getLevelsJson() {
        return levelsJson;
    }

    public void setLevelsJson(String levelsJson) {
        this.levelsJson = levelsJson;
    }

    public String getVariablesJson() {
        return variablesJson;
    }

    public void setVariablesJson(String variablesJson) {
        this.variablesJson = variablesJson;
    }

    public String getVariableValuesJson() {
        return variableValuesJson;
    }

    public void setVariableValuesJson(String variableValuesJson) {
        this.variableValuesJson = variableValuesJson;
    }

    public String getGamePlatformsJson() {
        return gamePlatformsJson;
    }

    public void setGamePlatformsJson(String gamePlatformsJson) {
        this.gamePlatformsJson = gamePlatformsJson;
    }

    public List<GameCategoryType> getGameCategoryTypes() {
        List<GameCategoryType> results = new ArrayList<GameCategoryType>();
        
        try {
            if (this.gameCategoryTypesJson != null) {
                results = Arrays.asList(_mapper.readValue(this.gameCategoryTypesJson, GameCategoryType[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
        
        return results;
    }    

    public List<Category> getCategories() {
        List<Category> results = new ArrayList<Category>();
        
        try {
            if (this.categoriesJson != null) {
                results = Arrays.asList(_mapper.readValue(this.categoriesJson, Category[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
        
        return results;
    }  

    public List<Level> getLevels() {
        List<Level> results = new ArrayList<Level>();
           
        try {
            if (this.levelsJson != null) {
                results = Arrays.asList(_mapper.readValue(this.levelsJson, Level[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
    
        return results;
    }
  
    public List<Variable> getVariables() {
        List<Variable> results = new ArrayList<Variable>();
        
        try {
            if (this.variablesJson != null) {
                results = Arrays.asList(_mapper.readValue(this.variablesJson, Variable[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
        
        return results;
    }

    public List<VariableValue> getVariableValues() {
        List<VariableValue> results = new ArrayList<VariableValue>();
        
        try {
            if (this.variableValuesJson != null) {
                results = Arrays.asList(_mapper.readValue(this.variableValuesJson, VariableValue[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
        
        return results;
    }

    public List<GamePlatform> getGamePlatforms() {
        List<GamePlatform> results = new ArrayList<GamePlatform>();
        
        try {
            if (this.gamePlatformsJson != null) {
                results = Arrays.asList(_mapper.readValue(this.gamePlatformsJson, GamePlatform[].class));
            }
        } catch (Exception ex) {
            throw new RuntimeException(ex);
        }
        
        return results;
    }      
}
