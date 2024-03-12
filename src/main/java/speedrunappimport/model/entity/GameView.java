package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;
import speedrunappimport.model.json.*;

import java.time.Instant;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Arrays;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

@Entity
@Table(name = "vw_game")
public class GameView {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String name;
    private String code;
    private String abbr;
    private String coverImageUrl;
    private String speedRunComUrl;
    private boolean showMilliseconds;
    private LocalDate releaseDate;
    private String categoryTypesJson;
    private String categoriesJson;
    private String levelsJson;
    private String variablesJson;
    private String variableValuesJson;
    private String platformsJson;

    @Transient
    private ObjectMapper _mapper;

    public GameView() {
        _mapper = new ObjectMapper().configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
                .setPropertyNamingStrategy(PropertyNamingStrategies.UPPER_CAMEL_CASE)
                .registerModule(new JavaTimeModule());
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

    public String getCoverImageUrl() {
        return coverImageUrl;
    }

    public void setCoverImageUrl(String coverImageUrl) {
        this.coverImageUrl = coverImageUrl;
    }

    public String getSpeedRunComUrl() {
        return speedRunComUrl;
    }

    public void setSpeedRunComUrl(String speedRunComUrl) {
        this.speedRunComUrl = speedRunComUrl;
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

    public String getCategoryTypesJson() {
        return categoryTypesJson;
    }

    public void setCategoryTypesJson(String categoryTypesJson) {
        this.categoryTypesJson = categoryTypesJson;
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

    public String getPlatformsJson() {
        return platformsJson;
    }

    public void setPlatformsJson(String platformsJson) {
        this.platformsJson = platformsJson;
    }

    public CategoryType[] getCategoryTypes() throws Exception {
        return _mapper.readValue(this.categoryTypesJson, CategoryType[].class);
    }    
}
