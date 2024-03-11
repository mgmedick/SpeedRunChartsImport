package speedrunappimport.model.json;

public record LevelResponse(String id,
String name,
String weblink,
String rules) {
}
/*
public class LevelResponse
{
    private String id;
    private String name;
    private String weblink;
    private String rules;

    public String getId() {
        return id;
    }
    public void setId(String id) {
        this.id = id;
    }
    public String getName() {
        return name;
    }
    public void setName(String name) {
        this.name = name;
    }
    public String getWeblink() {
        return weblink;
    }
    public void setWeblink(String weblink) {
        this.weblink = weblink;
    }
    public String getRules() {
        return rules;
    }
    public void setRules(String rules) {
        this.rules = rules;
    }    
}
*/
