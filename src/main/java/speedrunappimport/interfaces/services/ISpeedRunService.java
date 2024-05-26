package speedrunappimport.interfaces.services;

public interface ISpeedRunService
{
	boolean ProcessSpeedRuns(boolean isReload);
	void UpdateSpeedRunRanks(boolean isReload);
}
