package speedrunappimport.interfaces.jparepositories;

import java.util.List;
import java.time.Instant;

import org.springframework.data.jpa.repository.query.Procedure;
import org.springframework.data.repository.query.Param;
// import org.springframework.transaction.annotation.Transactional;

import speedrunappimport.model.entity.*;

public interface ISpeedRunDB extends IBaseDB<SpeedRun, Integer>
{
    public List<SpeedRun> findByCodeIn(List<String> codes);

    @Procedure("ImportUpdateSpeedRunRanks")
    // @Transactional(timeout = 32767) 
    void updateSpeedRunRanks(@Param("LastImportDate") Instant lastImportDateUtc);    

    @Procedure("ImportUpdateSpeedRunOrdered")
    // @Transactional(timeout = 32767) 
    void updateSpeedRunOrdered(@Param("LastImportDate") Instant lastImportDateUtc);       
}
