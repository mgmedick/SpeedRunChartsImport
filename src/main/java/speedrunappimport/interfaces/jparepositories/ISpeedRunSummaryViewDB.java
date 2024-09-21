package speedrunappimport.interfaces.jparepositories;

import java.time.Instant;
import java.util.List;

import org.springframework.data.jpa.repository.Query;

import speedrunappimport.model.entity.*;

public interface ISpeedRunSummaryViewDB extends IBaseDB<SpeedRunSummaryView, Integer>
{
    @Query("SELECT rn FROM vw_speedrunsummary rn WHERE COALESCE(rn.modifiedDate, rn.createdDate) > :compareDate")
    List<SpeedRunSummaryView> findAllWithModifiedDateAfter(Instant compareDate);     
}
