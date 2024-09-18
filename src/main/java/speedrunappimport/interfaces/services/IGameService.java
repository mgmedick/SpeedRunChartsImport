package speedrunappimport.interfaces.services;

import java.util.concurrent.CompletableFuture;

public interface IGameService
{
	boolean ProcessGames(boolean isReload);
	CompletableFuture<Boolean> RefreshCache();
}
