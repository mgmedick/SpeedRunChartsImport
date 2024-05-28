package speedrunappimport.model;

public class Enums {
    public enum CategoryTypes {
        PERGAME(0),
        PERLEVEL(1);

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
        FULLGAME(1),
        ALLLEVELS(2),
        SINGLELEVEL(3);

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
        GAMEDESC(1),
        CATEGORY(2),
        CATEGORYDESC(3),
        LEVEL(4),
        LEVELDESC(5),
        PLATFORM(6),
        PLATFORMDESC(7),
        REGION(8),
        REGIONDESC(9),
        EMULATED(10),
        EMULATEDDESC(11),
        DATE(12),
        DATEDESC(13),
        DATESUBMITTED(14),
        DATESUBMITTEDDESC(15),
        STATUS(16),
        STATUSDESC(17),
        VERIFYDATE(18),
        VERIFYDATEDESC(19);

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
        SIMILARITYDESC(1),
        NAME(2),
        NAMEDESC(3),
        JAPANESENAME(4),
        JAPANESENAMEDESC(5),
        ABBREVIATION(6),
        ABBREVIATIONDESC(7),
        YEAROFRELEASE(8),
        YEAROFRELEASEDESC(9),
        CREATIONDATE(10),
        CREATIONDATEDESC(11);

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