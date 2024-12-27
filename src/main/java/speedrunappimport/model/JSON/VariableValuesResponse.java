package speedrunappimport.model.json;

import java.util.LinkedHashMap;
import com.fasterxml.jackson.annotation.JsonProperty;

public record VariableValuesResponse(LinkedHashMap<String, VariableValueResponse> values,
@JsonProperty("default") String defaultId) {
}

