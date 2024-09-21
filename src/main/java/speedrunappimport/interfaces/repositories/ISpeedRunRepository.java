package speedrunappimport.interfaces.repositories;

import java.util.List;
import java.time.Instant;

import speedrunappimport.model.entity.*;

public interface ISpeedRunRepository {
    void SaveSpeedRuns(List<SpeedRun> runs);
    void SaveSpeedRunVideos(List<SpeedRunVideo> videos);
    List<SpeedRun> GetSpeedRunsByCode(List<String> codes);
    List<SpeedRunView> GetSpeedRunViewsByCode(List<String> codes);
    Instant GetMaxVerifyDate();
    List<SpeedRunSummaryView> GetSpeedRunSummaryViews();
    List<SpeedRunSummaryView> GetSpeedRunSummaryViewsModifiedAfter(Instant date);
    void DeleteObsoleteSpeedRuns(Instant lastImportDateUtc);
    void UpdateSpeedRunRanks(Instant lastImportDateUtc);
    void UpdateSpeedRunSummary(Instant lastImportDateUtc);
}


