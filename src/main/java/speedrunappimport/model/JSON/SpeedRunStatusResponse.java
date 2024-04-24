package speedrunappimport.model.json;

import java.time.Instant;

public record SpeedRunStatusResponse(String status,
String examiner,
Instant verifyDate) {
}


