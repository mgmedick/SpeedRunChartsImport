package speedrunappimport.model.json;

import com.fasterxml.jackson.annotation.JsonProperty;

public record GameRulesetResponse(boolean showMilliseconds,
boolean requireVerification,
boolean requireVideo,
String[] runTimes,
String defaultTime,
boolean emulatorsAllowed) {
}

/*
public class GameRulesetResponse
{
    @JsonProperty("show-milliseconds")
    private boolean showmilliseconds;

    @JsonProperty("require-verification")
    private boolean requireverification;

    @JsonProperty("require-video")
    private boolean requirevideo;

    @JsonProperty("run-times")
    private String[] runtimes;

    @JsonProperty("default-time")
    private String defaulttime;
    
    @JsonProperty("emulators-allowed")
    private boolean emulatorsallowed;

    public boolean isShowmilliseconds() {
        return showmilliseconds;
    }

    public void setShowmilliseconds(boolean showmilliseconds) {
        this.showmilliseconds = showmilliseconds;
    }

    public boolean isRequireverification() {
        return requireverification;
    }

    public void setRequireverification(boolean requireverification) {
        this.requireverification = requireverification;
    }

    public boolean isRequirevideo() {
        return requirevideo;
    }

    public void setRequirevideo(boolean requirevideo) {
        this.requirevideo = requirevideo;
    }

    public String[] getRuntimes() {
        return runtimes;
    }

    public void setRuntimes(String[] runtimes) {
        this.runtimes = runtimes;
    }

    public String getDefaulttime() {
        return defaulttime;
    }

    public void setDefaulttime(String defaulttime) {
        this.defaulttime = defaulttime;
    }

    public boolean isEmulatorsallowed() {
        return emulatorsallowed;
    }

    public void setEmulatorsallowed(boolean emulatorsallowed) {
        this.emulatorsallowed = emulatorsallowed;
    }

    
}
*/
