package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import java.time.Instant;
import java.time.LocalDate;

@Entity
@Table(name = "tbl_game")
public class Game {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)    
    private Integer id;
    private String name;
    private String code;
    private String abbr;
    private Boolean showmilliseconds;
    private LocalDate releasedate;
    private Instant importrefdate;
    private Instant createddate;
    private Instant modifieddate;

    @Transient
    public Level[] levels;

    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getCode() {
        return code;
    }

    public void setCode(String code) {
        this.code = code;
    }

    public String getAbbr() {
        return abbr;
    }

    public void setAbbr(String abbr) {
        this.abbr = abbr;
    }

    public Boolean getShowmilliseconds() {
        return showmilliseconds;
    }

    public void setShowmilliseconds(Boolean showmilliseconds) {
        this.showmilliseconds = showmilliseconds;
    }

    public LocalDate getReleasedate() {
        return releasedate;
    }

    public void setReleasedate(LocalDate releasedate) {
        this.releasedate = releasedate;
    }

    public Instant getImportrefdate() {
        return importrefdate;
    }

    public void setImportrefdate(Instant importrefdate) {
        this.importrefdate = importrefdate;
    }

    public Instant getCreateddate() {
        return createddate;
    }

    public void setCreateddate(Instant createddate) {
        this.createddate = createddate;
    }

    public Instant getModifieddate() {
        return modifieddate;
    }

    public void setModifieddate(Instant modifieddate) {
        this.modifieddate = modifieddate;
    }

    public Level[] getLevels() {
        return levels;
    }

    public void setLevels(Level[] levels) {
        this.levels = levels;
    }
}


