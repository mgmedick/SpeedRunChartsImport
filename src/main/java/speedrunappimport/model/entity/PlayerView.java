package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "vw_player")
public class PlayerView {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String name;
    private String code;
    private int playerTypeId;
    private int playerLinkId;
    private String profileImageUrl;
    private String srcUrl;
    private String twitchUrl;
    private String hitboxUrl;
    private String youtubeUrl;
    private String twitterUrl;

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

    public int getPlayerTypeId() {
        return playerTypeId;
    }

    public void setPlayerTypeId(int playerTypeId) {
        this.playerTypeId = playerTypeId;
    }

    public int getPlayerLinkId() {
        return playerLinkId;
    }

    public void setPlayerLinkId(int playerLinkId) {
        this.playerLinkId = playerLinkId;
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
}
