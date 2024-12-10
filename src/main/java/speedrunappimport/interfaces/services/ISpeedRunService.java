package speedrunappimport.interfaces.services;

public interface ISpeedRunService
{
	boolean ProcessSpeedRuns(boolean isReload);
	boolean UpdateSpeedRunVideos(boolean isReload);
	boolean RenameFullTables();
}
