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
}