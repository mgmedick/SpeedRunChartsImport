package speedrunappimport.model.json;

import java.util.LinkedHashMap;

public record YoutubeSnippetResponse(String channelId,
LinkedHashMap<String, YoutubeThumbnailResponse> thumbnails) {
}

