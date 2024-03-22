package speedrunappimport.model.json;

public record CategoryResponse(String id,
String name,
String weblink,
String type,
String rules,
CategoryPlayersResponse players,
boolean miscellaneous) {
}

