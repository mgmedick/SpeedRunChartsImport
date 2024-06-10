package speedrunappimport.interfaces.repositories;

import java.util.List;
import java.time.Instant;

import speedrunappimport.model.entity.*;

public interface ISpeedRunRepository {
    void SaveSpeedRuns(List<SpeedRun> runs);
    List<SpeedRun> GetSpeedRunsByCode(List<String> codes);
    List<SpeedRunView> GetSpeedRunViewsByCode(List<String> codes);
    void UpdateSpeedRunRanks(Instant lastImportDateUtc);
    void UpdateSpeedRunOrdered(Instant lastImportDateUtc);
}


