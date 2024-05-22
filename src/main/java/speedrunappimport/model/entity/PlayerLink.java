package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "tbl_player_link")
public class PlayerLink {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private int playerId;
    private String profileImageUrl;
    private String srcUrl;
    private String twitchUrl;
    private String hitboxUrl;
    private String youtubeUrl;
    private String twitterUrl;
    private String speedRunsLiveUrl;

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public int getPlayerId() {
        return playerId;
    }

    public void setPlayerId(int playerId) {
        this.playerId = playerId;
    }

    public String getProfileImageUrl() {
        return profileImageUrl;
    }

    public void setProfileImageUrl(String profileImageUrl) {
        this.profileImageUrl = profileImageUrl;
    }

    public String getSrcUrl() {
        return srcUrl;
    }

    public void setSrcUrl(String srcUrl) {
        this.srcUrl = srcUrl;
    }

    public String getTwitchUrl() {
        return twitchUrl;
    }

    public void setTwitchUrl(String twitchUrl) {
        this.twitchUrl = twitchUrl;
    }

    public String getHitboxUrl() {
        return hitboxUrl;
    }

    public void setHitboxUrl(String hitboxUrl) {
        this.hitboxUrl = hitboxUrl;
    }

    public String getYoutubeUrl() {
        return youtubeUrl;
    }

    public void setYoutubeUrl(String youtubeUrl) {
        this.youtubeUrl = youtubeUrl;
    }

    public String getTwitterUrl() {
        return twitterUrl;
    }

    public void setTwitterUrl(String twitterUrl) {
        this.twitterUrl = twitterUrl;
    }

    public String getSpeedRunsLiveUrl() {
        return speedRunsLiveUrl;
    }

    public void setSpeedRunsLiveUrl(String speedRunsLiveUrl) {
        this.speedRunsLiveUrl = speedRunsLiveUrl;
    }
}
