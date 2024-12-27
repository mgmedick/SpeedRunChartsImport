package speedrunappimport.model.json;

import java.time.Instant;
import java.time.LocalDate;
import java.util.List;
import java.util.LinkedHashMap;

public record GameResponse(String id,
GameNameResponse names,
String abbreviation,
String weblink,
Integer released,
LocalDate releaseDate,
GameRulesetResponse ruleset,
boolean romhack,
List<String> gametypes,
List<String> platforms,
List<String> genres,
List<String> engines,
List<String> developers,
List<String> publishers,
LinkedHashMap<String, String> moderators,
Instant created,
GameAssetsResponse assets,
GameCategoryResponse categories,
GameLevelResponse levels,
GameVariableResponse variables) {
}


