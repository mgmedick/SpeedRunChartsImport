package speedrunappimport.model.JSON;

public class GameResponse {
    public String id;
    public GameNameResponse names;
    public String abbreviation;
    public String created;

    public String getId() {
        return id;
    }

    public void setId(String id) {
        this.id = id;
    }

    public GameNameResponse getNames() {
        return names;
    }

    public void setNames(GameNameResponse names) {
        this.names = names;
    }

    public String getAbbreviation() {
        return abbreviation;
    }

    public void setAbbreviation(String abbreviation) {
        this.abbreviation = abbreviation;
    }  

    public String getCreated() {
        return created;
    }

    public void setCreated(String created) {
        this.created = created;
    }      
}
