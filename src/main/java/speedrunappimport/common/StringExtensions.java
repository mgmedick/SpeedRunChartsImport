package speedrunappimport.common;

import java.util.regex.Pattern;

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
}
