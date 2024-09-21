package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import java.time.Instant;

import org.hibernate.annotations.*;

@Entity
@Table(name = "tbl_speedrun_video")
@SQLDelete(sql = "UPDATE tbl_speedrun_video SET deleted = true WHERE id=?")
@SQLRestriction("deleted = false")
public class SpeedRunVideo implements Cloneable {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private int speedRunId;
    private String videoLinkUrl;
    private String embeddedVideoLinkUrl;
    private String thumbnailLinkUrl;
    private String channelCode;
    private Long viewCount;
    private boolean deleted;

    public SpeedRunVideo()
    {       
    }

    public SpeedRunVideo(int id, int speedRunId, String videoLinkUrl, String embeddedVideoLinkUrl,
            String thumbnailLinkUrl, String channelCode, Long viewCount, boolean deleted) {
        this.id = id;
        this.speedRunId = speedRunId;
        this.videoLinkUrl = videoLinkUrl;
        this.embeddedVideoLinkUrl = embeddedVideoLinkUrl;
        this.thumbnailLinkUrl = thumbnailLinkUrl;
        this.channelCode = channelCode;
        this.viewCount = viewCount;
        this.deleted = deleted;
    }

    @Transient
    private String videoId;

    public int getId() {
        return id;
    }
    public void setId(int id) {
        this.id = id;
    }
    public int getSpeedRunId() {
        return speedRunId;
    }
    public void setSpeedRunId(int speedRunId) {
        this.speedRunId = speedRunId;
    }
    public String getVideoLinkUrl() {
        return videoLinkUrl;
    }
    public void setVideoLinkUrl(String videoLinkUrl) {
        this.videoLinkUrl = videoLinkUrl != null && videoLinkUrl.length() > 500 ? videoLinkUrl.substring(0,500) : videoLinkUrl;
    }
    public String getEmbeddedVideoLinkUrl() {
        return embeddedVideoLinkUrl;
    }
    public void setEmbeddedVideoLinkUrl(String embeddedVideoLinkUrl) {
        this.embeddedVideoLinkUrl = embeddedVideoLinkUrl != null && embeddedVideoLinkUrl.length() > 500 ? embeddedVideoLinkUrl.substring(0,500) : embeddedVideoLinkUrl;
    }
    public String getThumbnailLinkUrl() {
        return thumbnailLinkUrl;
    }
    public void setThumbnailLinkUrl(String thumbnailLinkUrl) {
        this.thumbnailLinkUrl = thumbnailLinkUrl != null && thumbnailLinkUrl.length() > 500 ? thumbnailLinkUrl.substring(0,500) : thumbnailLinkUrl;
    }
    public String getChannelCode() {
        return channelCode;
    }
    public void setChannelCode(String channelCode) {
        this.channelCode = channelCode;
    }
    public Long getViewCount() {
        return viewCount;
    }
    public void setViewCount(Long viewCount) {
        this.viewCount = viewCount;
    }
    public boolean isDeleted() {
        return deleted;
    }
    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    }
    public String getVideoId() {
        return videoId;
    }
    public void setVideoId(String videoId) {
        this.videoId = videoId;
    }
      
    @Override
    public Object clone() {
        try {
            return (SpeedRunVideo) super.clone();
        } catch (CloneNotSupportedException e) {
            return new SpeedRunVideo(this.getId(), this.getSpeedRunId(), this.getVideoLinkUrl(), this.getEmbeddedVideoLinkUrl(), this.getThumbnailLinkUrl(), this.getChannelCode(), this.getViewCount(), this.isDeleted());            
        }
    } 
}
