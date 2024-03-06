package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "tbl_level")
public class Level {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)    
    public Integer id;
    public String name;
    public String code;
    public Integer gameid;
}


