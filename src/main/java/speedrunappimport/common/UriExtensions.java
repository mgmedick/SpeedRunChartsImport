package speedrunappimport.common;

import java.net.URI;
import java.net.URLDecoder;
import java.nio.charset.StandardCharsets;
import java.util.LinkedHashMap;
import java.util.Map;

public class UriExtensions
{
    public static String ToEmbeddedURIString(String uriString)
    {
        String result = null;

        if (uriString != null && !uriString.isBlank()) {
            var uriResult = ToEmbeddedURI(URI.create(uriString));
            if (uriResult != null) {
                result = uriResult.toString();
            }
        }
        
        return result;
    }

    public static URI ToEmbeddedURI(URI uri)
    {
        URI embededURI = null;

        if (uri != null)
        {
            var domain = uri.getHost();
            var path = uri.getPath();
            var segments = path.split("/");
            String videoIDString = null;
            String uriString = null;

            if (domain.contains("twitch.tv"))
            {
                if (path.startsWith("/videos/"))
                {
                    videoIDString = segments[segments.length-1];
                    uriString = String.format("https://player.twitch.tv/?video=%s&parent=localhost&parent=speedruncharts.com&parent=www.speedruncharts.com&autoplay=false&muted=true", videoIDString);
                }
            }
            else if (domain.contains("youtube.com") || domain.contains("youtu.be"))
            {
                var queryDictionary = splitQuery(uri);
                videoIDString = queryDictionary.containsKey("v") ? queryDictionary.get("v") : segments[segments.length-1];
                uriString = String.format("https://www.youtube.com/embed/%s?autoplay=0&mute=1", videoIDString);
            }
            else if (domain.contains("vimeo.com"))
            {
                if (path.startsWith("/video/"))
                {
                    videoIDString = segments[segments.length-1];
                    uriString = String.format("https://player.vimeo.com/video/%s?autoplay=0&muted=1", videoIDString);
                }
            }
            else if (domain.contains("streamable.com"))
            {
                videoIDString = segments[segments.length-1];
                uriString = String.format("https://streamable.com/o/%s", videoIDString);
            }
            else if (domain.contains("medal.tv"))
            {
                uriString = String.format("https://medal.tv/clip/%s/%s?autoplay=0&muted=1&loop=0", segments[segments.length-2], segments[segments.length-1]);
            }

            if (uriString != null && !uriString.isBlank())
            {
                embededURI = URI.create(uriString);
            }
        }

        return embededURI;
    }

    public static String ToThumbnailURIString(String uriString)
    {
        String result = null;

        if (uriString != null && !uriString.isBlank()) {
            var uriResult = ToThumbnailURI(URI.create(uriString));
            if (uriResult != null) {
                result = uriResult.toString();
            }
        }
        
        return result;
    }

    public static URI ToThumbnailURI(URI uri)
    {
        URI embededURI = null;

        if (uri != null)
        {
            var domain = uri.getHost();
            var path = uri.getPath();
            var segments = path.split("/");
            String videoIDString = null;
            String uriString = null;

            if (domain.contains("youtube.com") || domain.contains("youtu.be"))
            {
                var queryDictionary = splitQuery(uri);
                if (queryDictionary.containsKey("v")) {
                    videoIDString = queryDictionary.get("v");
                } else if (segments.length > 0) {
                    videoIDString = segments[segments.length - 1];
                }
                
                uriString = String.format("https://img.youtube.com/vi/%s/hqdefault.jpg", videoIDString);
            }

            if (uriString != null && !uriString.isBlank())
            {
                embededURI = URI.create(uriString);
            }
        }

        return embededURI;
    }

    public static Map<String, String> splitQuery(URI uri) {
        Map<String, String> query_pairs = new LinkedHashMap<String, String>();
        String query = uri.getQuery();
        if (query != null && !query.isBlank()) {
            String[] pairs = query.split("&");
            for (String pair : pairs) {
                int idx = pair.indexOf("=");
                var key = idx > 0 ? URLDecoder.decode(pair.substring(0, idx), StandardCharsets.UTF_8) : pair;
                var value = idx > 0 && pair.length() > idx + 1 ? URLDecoder.decode(pair.substring(idx + 1), StandardCharsets.UTF_8) : null;
                query_pairs.put(key, value);
            }
        }

        return query_pairs;
    }
}
