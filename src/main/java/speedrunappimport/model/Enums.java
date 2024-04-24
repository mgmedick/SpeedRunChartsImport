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

    public enum SpeedRunOrderBy {
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
 
        private SpeedRunOrderBy(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }        
    } 
}