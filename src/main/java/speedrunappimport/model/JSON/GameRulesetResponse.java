package speedrunappimport.model.json;

import com.fasterxml.jackson.annotation.JsonProperty;

public class GameRulesetResponse
{
    @JsonProperty("show-milliseconds")
    public boolean showMilliseconds;

    @JsonProperty("require-verification")
    public boolean requireVerification;

    @JsonProperty("require-video")
    public boolean requireVideo;

    @JsonProperty("run-times")
    public String[] runTimes;

    @JsonProperty("default-time")
    public String defaultTime;
    
    @JsonProperty("emulators-allowed")
    public boolean emulatorsAllowed;
}
