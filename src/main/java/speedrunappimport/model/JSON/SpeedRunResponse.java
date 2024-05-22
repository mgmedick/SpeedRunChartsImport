package speedrunappimport.model.json;

import java.time.Instant;
import java.time.LocalDate;
import java.util.HashMap;
import java.util.List;

public record SpeedRunResponse(String id,
String weblink,
String game,
String level,
String category,
SpeedRunVideoResponse videos,
SpeedRunStatusResponse status,
List<PlayerResponse> players,
LocalDate date,
Instant submitted,
SpeedRunTimeResponse times,
SpeedRunSystemResponse system,
List<LinkResponse> splits,
HashMap<String, String> values) {
}


