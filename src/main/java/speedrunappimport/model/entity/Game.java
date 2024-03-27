package speedrunappimport.model.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import java.time.Instant;
import java.time.LocalDate;
import java.util.List;

@Entity
@Table(name = "tbl_game")
public class Game {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    private String name;
    private String code;
    private String abbr;
    private boolean showMilliseconds;
    private LocalDate releaseDate;
    private Instant importRefDate;
    private Instant modifiedDate;

    @Transient
    private Instant createdDate;

    @Transient
    private List<CategoryType> categoryTypes;

    @Transient
    private List<Category> categories;    

    @Transient
    private List<Level> levels;

    @Transient
    private List<Variable> variables;

    @Transient
    private List<VariableValue> variableValues;  
    
    @Transient
    private List<Integer> categoriesToRemove;    

    @Transient
    private List<Integer> levelsToRemove;       

    @Transient
    private List<Integer> variablesToRemove;       

    @Transient
    private List<Integer> variableValuesToRemove;  

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

    public String getAbbr() {
        return abbr;
    }

    public void setAbbr(String abbr) {
        this.abbr = abbr;
    }

    public boolean isShowMilliseconds() {
        return showMilliseconds;
    }

    public void setShowMilliseconds(boolean showMilliseconds) {
        this.showMilliseconds = showMilliseconds;
    }

    public LocalDate getReleaseDate() {
        return releaseDate;
    }

    public void setReleaseDate(LocalDate releaseDate) {
        this.releaseDate = releaseDate;
    }

    public Instant getImportRefDate() {
        return importRefDate;
    }

    public void setImportRefDate(Instant importRefDate) {
        this.importRefDate = importRefDate;
    }

    public Instant getCreatedDate() {
        return createdDate;
    }

    public void setCreatedDate(Instant createdDate) {
        this.createdDate = createdDate;
    }

    public Instant getModifiedDate() {
        return modifiedDate;
    }

    public void setModifiedDate(Instant modifiedDate) {
        this.modifiedDate = modifiedDate;
    }

    public List<CategoryType> getCategoryTypes() {
        return categoryTypes;
    }

    public void setCategoryTypes(List<CategoryType> categoryTypes) {
        this.categoryTypes = categoryTypes;
    }

    public List<Category> getCategories() {
        return categories;
    }

    public void setCategories(List<Category> categories) {
        this.categories = categories;
    }

    public List<Level> getLevels() {
        return levels;
    }

    public void setLevels(List<Level> levels) {
        this.levels = levels;
    }

    public List<Variable> getVariables() {
        return variables;
    }

    public void setVariables(List<Variable> variables) {
        this.variables = variables;
    }

    public List<VariableValue> getVariableValues() {
        return variableValues;
    }

    public void setVariableValues(List<VariableValue> variableValues) {
        this.variableValues = variableValues;
    }

    public List<Integer> getCategoriesToRemove() {
        return categoriesToRemove;
    }

    public void setCategoriesToRemove(List<Integer> categoriesToRemove) {
        this.categoriesToRemove = categoriesToRemove;
    }

    public List<Integer> getLevelsToRemove() {
        return levelsToRemove;
    }

    public void setLevelsToRemove(List<Integer> levelsToRemove) {
        this.levelsToRemove = levelsToRemove;
    }

    public List<Integer> getVariablesToRemove() {
        return variablesToRemove;
    }

    public void setVariablesToRemove(List<Integer> variablesToRemove) {
        this.variablesToRemove = variablesToRemove;
    }

    public List<Integer> getVariableValuesToRemove() {
        return variableValuesToRemove;
    }

    public void setVariableValuesToRemove(List<Integer> variableValuesToRemove) {
        this.variableValuesToRemove = variableValuesToRemove;
    }
}
