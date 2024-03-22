package speedrunappimport.model.json;

import java.util.HashMap;
import com.fasterxml.jackson.annotation.JsonProperty;

public record VariableValuesResponse(HashMap<String, VariableValueResponse> values,
@JsonProperty("default") String defaultId) {
}

