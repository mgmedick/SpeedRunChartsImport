package speedrunappimport.model.Data;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import java.time.Instant;

@Entity
@Table(name = "tbl_game")
public class Game {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)    
    public int id;
    public String name;
    public Boolean isromhack;
    public Integer yearofrelease;
    public String abbr;
    public Instant createddate;
    public Instant importeddate;
    public Instant modifieddate;
    public Boolean ischanged;
}


