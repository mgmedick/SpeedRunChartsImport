package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import java.time.Instant;

@Entity
@Table(name = "tbl_setting")
public class Setting {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)    
    private int id;
    private String name;
    private String str;
    private Integer num;
    private Instant dte;
  
    public int getId() {
        return id;
    }
    public void setId(int id) {
        this.id = id;
    }
    public String getName() {
        return name;
    }
    public void setName(String name) {
        this.name = name;
    }
    public String getStr() {
        return str;
    }
    public void setStr(String str) {
        this.str = str;
    }
    public Integer getNum() {
        return num;
    }
    public void setNum(Integer num) {
        this.num = num;
    }
    public Instant getDte() {
        return dte;
    }
    public void setDte(Instant dte) {
        this.dte = dte;
    }
}


