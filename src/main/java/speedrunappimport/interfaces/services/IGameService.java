package speedrunappimport.interfaces.services;

import java.time.Instant;

public interface IGameService
{
	boolean ProcessGames(Instant lastImportDateUtc, boolean isReload);
}
