package speedrunappimport.interfaces.repositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface ISpeedRunRepository {
    void SaveSpeedRuns(List<SpeedRun> runs);
    List<SpeedRun> GetSpeedRunsByCode(List<String> codes);
    List<SpeedRunView> GetSpeedRunViewsByCode(List<String> codes);
}


