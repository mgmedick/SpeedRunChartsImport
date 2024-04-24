package speedrunappimport.model.json;

public record SpeedRunTimeResponse(String primary,
long primary_t,
String realtime,
long realtime_t,
String realtime_noloads,
long realtime_noloads_t,
String ingame,
long ingame_t) {
}


