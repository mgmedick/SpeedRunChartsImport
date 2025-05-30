package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import org.hibernate.annotations.*;

@Entity
@Table(name = "tbl_variable")
@SQLDelete(sql = "UPDATE tbl_variable SET deleted = true WHERE id=?")
@SQLRestriction("deleted = false")
public class Variable {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String name;
    private String code;
    private int gameId;
    private int variableScopeTypeId;
    private Integer categoryId;
    private Integer levelId;   
    private boolean isSubCategory;
    private Integer sortOrder;
    private boolean deleted;

    @Transient
    private String categoryCode;

    @Transient
    private String levelCode;

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

    public String getCode() {
        return code;
    }

    public void setCode(String code) {
        this.code = code;
    }

    public int getGameId() {
        return gameId;
    }

    public void setGameId(int gameId) {
        this.gameId = gameId;
    }

    public int getVariableScopeTypeId() {
        return variableScopeTypeId;
    }

    public void setVariableScopeTypeId(int variableScopeTypeId) {
        this.variableScopeTypeId = variableScopeTypeId;
    }

    public Integer getCategoryId() {
        return categoryId;
    }

    public void setCategoryId(Integer categoryId) {
        this.categoryId = categoryId;
    }

    public Integer getLevelId() {
        return levelId;
    }

    public void setLevelId(Integer levelId) {
        this.levelId = levelId;
    }

    public boolean isSubCategory() {
        return isSubCategory;
    }

    public void setIsSubCategory(boolean isSubCategory) {
        this.isSubCategory = isSubCategory;
    }

    public Integer getSortOrder() {
        return sortOrder;
    }

    public void setSortOrder(Integer sortOrder) {
        this.sortOrder = sortOrder;
    }    
     
    public String getCategoryCode() {
        return categoryCode;
    }

    public void setCategoryCode(String categoryCode) {
        this.categoryCode = categoryCode;
    }

    public String getLevelCode() {
        return levelCode;
    }

    public void setLevelCode(String levelCode) {
        this.levelCode = levelCode;
    }

    public boolean isDeleted() {
        return deleted;
    }

    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    }      
}
