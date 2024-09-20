package speedrunappimport.model.json;

import java.util.HashMap;

public record YoutubeSnippetResponse(String channelId,
HashMap<String, YoutubeThumbnailResponse> thumbnails) {
}

