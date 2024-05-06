package speedrunappimport.model;

public class Enums {
    public enum CategoryType {
        PerGame(0),
        PerLevel(1);

        private final int value;
 
        private CategoryType(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }
    }
    
    public enum VariableScopeType {
        Global(0),
        FullGame(1),
        AllLevels(2),
        SingleLevel(3);

        private final int value;
 
        private VariableScopeType(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }        
    }

    public enum SpeedRunsOrderBy {
        Game(0),
        GameDesc(1),
        Category(2),
        CategoryDesc(3),
        Level(4),
        LevelDesc(5),
        Platform(6),
        PlatformDesc(7),
        Region(8),
        RegionDesc(9),
        Emulated(10),
        EmulatedDesc(11),
        Date(12),
        DateDesc(13),
        DateSubmitted(14),
        DateSubmittedDesc(15),
        Status(16),
        StatusDesc(17),
        VerifyDate(18),
        VerifyDateDesc(19);

        private final int value;
 
        private SpeedRunsOrderBy(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }        
    }

    public enum GamesOrderBy {
        Similarity(0),
        SimilarityDesc(1),
        Name(2),
        NameDesc(3),
        JapaneseName(4),
        JapaneseNamelDesc(5),
        Abbreviation(6),
        AbbreviationDesc(7),
        YearOfRelease(8),
        YearOfReleaseDesc(9),
        CreationDate(10),
        CreationDateDesc(11);

        private final int value;
 
        private GamesOrderBy(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }        
    } 
}