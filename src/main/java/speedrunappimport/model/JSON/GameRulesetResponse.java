package speedrunappimport.model.json;

import java.util.List;

public record GameRulesetResponse(boolean showMilliseconds,
boolean requireVerification,
boolean requireVideo,
List<String> runTimes,
String defaultTime,
boolean emulatorsAllowed) {
}
