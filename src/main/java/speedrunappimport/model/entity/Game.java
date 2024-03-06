package speedrunappimport.model.entity;

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
    public Integer id;
    public String name;
    public String code;
    public String abbr;
    public Boolean isromhack;
    public Integer yearofrelease;
    public Instant importrefdate;
    public Instant createddate;
    public Instant modifieddate;
}


