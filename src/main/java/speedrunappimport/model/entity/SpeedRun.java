package speedrunappimport.model.entity;

import java.time.Instant;
import java.util.List;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import org.hibernate.annotations.*;

@Entity
@Table(name = "tbl_speedrun")
@SQLDelete(sql = "UPDATE tbl_speedrun SET deleted = true WHERE id=?")
@SQLRestriction("deleted = false")
public class SpeedRun {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String code;
    private int gameId;
    private int categoryTypeId;
    private int categoryId;
    private Integer levelId;
    private Integer platformId;
    @Column(name="`rank`")
    private Integer rank;
    private long primaryTime;
    private Instant dateSubmitted;
    private Instant verifyDate;
    @Transient
    private Instant createdDate;
    private Instant modifiedDate;

    @Transient
    private SpeedRunLink speedRunLink;

    @Transient
    private List<SpeedRunPlayer> players;   

    @Transient
    private List<SpeedRunVariableValue> variableValues;

    @Transient
    private List<SpeedRunVideo> videos;  

    @Transient
    private List<Integer> playersToRemove;    

    @Transient
    private List<Integer> variableValuesToRemove;    

    @Transient
    private List<Integer> videosToRemove;    

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
    public void setDateSumbitted(Instant dateSubmitted) {
        this.dateSubmitted = dateSubmitted;
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
    public SpeedRunLink getSpeedRunLink() {
        return speedRunLink;
    }
    public void setSpeedRunLink(SpeedRunLink speedRunLink) {
        this.speedRunLink = speedRunLink;
    }
    public List<SpeedRunPlayer> getPlayers() {
        return players;
    }
    public void setPlayers(List<SpeedRunPlayer> players) {
        this.players = players;
    }
    public List<SpeedRunVariableValue> getVariableValues() {
        return variableValues;
    }
    public void setVariableValues(List<SpeedRunVariableValue> variableValues) {
        this.variableValues = variableValues;
    }
    public List<SpeedRunVideo> getVideos() {
        return videos;
    }
    public void setVideos(List<SpeedRunVideo> videos) {
        this.videos = videos;
    }
    public List<Integer> getPlayersToRemove() {
        return playersToRemove;
    }
    public void setPlayersToRemove(List<Integer> playersToRemove) {
        this.playersToRemove = playersToRemove;
    }    
    public List<Integer> getVariableValuesToRemove() {
        return variableValuesToRemove;
    }
    public void setVariableValuesToRemove(List<Integer> variableValuesToRemove) {
        this.variableValuesToRemove = variableValuesToRemove;
    }
    public List<Integer> getVideosToRemove() {
        return videosToRemove;
    }
    public void setVideosToRemove(List<Integer> videosToRemove) {
        this.videosToRemove = videosToRemove;
    }
}
