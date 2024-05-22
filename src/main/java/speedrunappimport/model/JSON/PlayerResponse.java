package speedrunappimport.model.json;

import java.time.Instant;
import java.util.List;

public record PlayerResponse(String rel,
String id,
String name,
NameResponse names,
boolean supporterAnimation,
String pronouns,
String weblink,
PlayerNameStyleResponse nameStyle,
String role,
Instant signup,
LocationResponse location,
LinkResponse twich,
LinkResponse hitbox,
LinkResponse youtube,
LinkResponse twitter,
LinkResponse speedrunslive,
List<LinkResponse> links) {
}


