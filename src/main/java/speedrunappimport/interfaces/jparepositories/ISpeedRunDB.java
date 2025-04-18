package speedrunappimport.interfaces.jparepositories;

import java.util.List;
import java.time.Instant;

import org.springframework.data.jpa.repository.Query;
import org.springframework.data.jpa.repository.query.Procedure;
import org.springframework.data.repository.query.Param;
// import org.springframework.transaction.annotation.Transactional;

import speedrunappimport.model.entity.*;

public interface ISpeedRunDB extends IBaseDB<SpeedRun, Integer>
{
    public List<SpeedRun> findByCodeIn(List<String> codes);
    
    @Query("SELECT MAX(verifyDate) FROM tbl_speedrun")
    Instant findMaxVerifyDate();

    @Procedure("ImportDeleteObsoleteSpeedRuns")
    // @Transactional(timeout = 32767) 
    void deleteObsoleteSpeedRuns(@Param("LastImportDate") Instant lastImportDateUtc); 

    @Procedure("ImportUpdateSpeedRunRanks")
    // @Transactional(timeout = 32767) 
    void updateSpeedRunRanks(@Param("LastImportDate") Instant lastImportDateUtc);    

    @Procedure("ImportUpdateSpeedRunSummary")
    // @Transactional(timeout = 32767) 
    void updateSpeedRunSummary(@Param("LastImportDate") Instant lastImportDateUtc);     
    
    @Procedure("ImportRenameFullTables")
    // @Transactional(timeout = 32767) 
    void importRenameFullTables();    
   
}
