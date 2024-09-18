package speedrunappimport.common;

import java.util.regex.Pattern;
import java.nio.charset.StandardCharsets;
import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import java.util.Map;
import java.net.URLEncoder;

public class StringExtensions
{
    public static String KebabToUpperSnakeCase(String text)
    {
        String result = text;
        
        if (result != null && !result.isBlank()) {
            result = result.toUpperCase().replace("-", "_");         
        }

        return result;
    }

    public static String KebabToUpperCamelCase(String text)
    {
        String result = text;
        
        if (result != null && !result.isBlank()) {
            result = result.toLowerCase();
            result = Pattern.compile("-([a-z])")
                        .matcher(result)
                        .replaceAll(mr -> mr.group(1).toUpperCase());
            result = result.substring(0, 1).toUpperCase() + result.substring(1);            
        }

        return result;
    }

    public static String GetHMACSHA256Hash(String plaintext, String salt) {
        String result = "";
        try {
            byte[] baText2BeHashed = plaintext.getBytes(StandardCharsets.UTF_8);
            byte[] baSalt = salt.getBytes(StandardCharsets.UTF_8);
            Mac hasher = Mac.getInstance("HmacSHA256");
            SecretKeySpec keySpec = new SecretKeySpec(baSalt, "HmacSHA256");
            hasher.init(keySpec);
            byte[] baHashedText = hasher.doFinal(baText2BeHashed);
            StringBuilder sb = new StringBuilder();
            for (byte b : baHashedText) {
                sb.append(String.format("%02x", b));
            }
            result = sb.toString();
        } catch (Exception e) {
            e.printStackTrace();
        }
        return result;
    }

    public static String GetFormUrlEncodedString(Map<String, String> params) {
        StringBuilder result = new StringBuilder();
        for(Map.Entry<String,String> entry : params.entrySet()) {
            if (result.length() != 0) {
                result.append("&");
            }
            result.append(URLEncoder.encode(entry.getKey(), StandardCharsets.UTF_8));
            result.append("=");
            result.append(URLEncoder.encode(entry.getValue(), StandardCharsets.UTF_8));
        }
        return result.toString();
    }
}
