package speedrunappimport.model.json;

public record VariableResponse(String id,
String name,
String category,
VariableScopeResponse scope,
boolean mandatory,
boolean userDefined,
boolean obsoletes,
VariableValuesResponse values,
boolean isSubcategory) {
}

