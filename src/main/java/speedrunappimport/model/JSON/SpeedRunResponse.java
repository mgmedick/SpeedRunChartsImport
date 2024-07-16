package speedrunappimport.model.json;

import java.time.Instant;
import java.time.LocalDate;
import java.util.HashMap;

public record SpeedRunResponse(String id,
String weblink,
String game,
String level,
String category,
SpeedRunVideoResponse videos,
SpeedRunStatusResponse status,
SpeedRunPlayerResponse players,
LocalDate date,
Instant submitted,
SpeedRunTimeResponse times,
SpeedRunSystemResponse system,
// LinkResponse splits,
HashMap<String, String> values) {
}


