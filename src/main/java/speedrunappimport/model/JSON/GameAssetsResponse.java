package speedrunappimport.model.json;

public record GameAssetsResponse(GameAssetResponse logo,
GameAssetResponse coverTiny,
GameAssetResponse coverSmall,
GameAssetResponse coverMedium,
GameAssetResponse coverLarge) {
}
