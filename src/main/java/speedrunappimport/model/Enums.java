package speedrunappimport.model;

public class Enums {
    public enum CategoryTypes {
        PER_GAME(0),
        PER_LEVEL(1);

        private final int value;
 
        private CategoryTypes(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }  
    }
    
    public enum VariableScopeType {
        GLOBAL(0),
        FULL_GAME(1),
        ALL_LEVELS(2),
        SINGLE_LEVEL(3);

        private final int value;
 
        private VariableScopeType(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }        
    }

    public enum SpeedRunsOrderBy {
        GAME(0),
        GAME_DESC(1),
        CATEGORY(2),
        CATEGORY_DESC(3),
        LEVEL(4),
        LEVEL_DESC(5),
        PLATFORM(6),
        PLATFORM_DESC(7),
        REGION(8),
        REGION_DESC(9),
        EMULATED(10),
        EMULATED_DESC(11),
        DATE(12),
        DATE_DESC(13),
        SUBMITTED(14),
        SUBMITTED_DESC(15),
        STATUS(16),
        STATUS_DESC(17),
        VERIFY_DATE(18),
        VERIFY_DATE_DESC(19);

        private final int value;
 
        private SpeedRunsOrderBy(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }        
    }

    public enum GamesOrderBy {
        SIMILARITY(0),
        SIMILARITY_DESC(1),
        NAME(2),
        NAME_DESC(3),
        JAPANESE_NAME(4),
        JAPANESE_NAME_DESC(5),
        ABBREVIATION(6),
        ABBREVIATION_DESC(7),
        YEAR_OF_RELEASE(8),
        YEAR_OF_RELEASE_DESC(9),
        CREATION_DATE(10),
        CREATION_DATE_DESC(11);

        private final int value;
 
        private GamesOrderBy(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }        
    }
    
    public enum PlayerType {
        USER(0),
        GUEST(1);

        private final int value;
 
        private PlayerType(int value) {
            this.value = value;
        }
    
        public int getValue() {
            return value;
        }
    }    
}