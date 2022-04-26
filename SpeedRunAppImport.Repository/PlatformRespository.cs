using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Repository
{
    public class PlatformRespository : BaseRepository, IPlatformRepository
    {
        private readonly ILogger _logger;

        public PlatformRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void InsertPlatforms(IEnumerable<PlatformEntity> platforms)
        {
            _logger.Information("Started InsertPlatforms");
            int batchCount = 0;
            var platformsList = platforms.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                while (batchCount < platformsList.Count)
                {
                    var platformsBatch = platformsList.Skip(batchCount).Take(MaxBulkRows).ToList();

                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBatch<PlatformEntity>(platformsBatch);
                        var platformIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_Platform_Full ORDER BY ID DESC LIMIT @0;", platformsBatch.Count).Reverse().ToArray() :
                                                    db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Platform_Full ORDER BY ID DESC", platformsBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < platformsBatch.Count; i++)
                        {
                            platformsBatch[i].ID = platformIDs[i];
                        }

                        var platformSpeedRunComIDsBatch = platformsBatch.Select(i => new PlatformSpeedRunComIDEntity { PlatformID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<PlatformSpeedRunComIDEntity>(platformSpeedRunComIDsBatch);

                        tran.Complete();
                    }

                    _logger.Information("Saved platforms {@Count} / {@Total}", platformsBatch.Count, platformsList.Count);
                    batchCount += MaxBulkRows;
                }
            }
            _logger.Information("Completed InsertPlatforms");
        }

        public void SavePlatforms(IEnumerable<PlatformEntity> platforms)
        {
            int count = 1;
            var platformsList = platforms.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                foreach (var platform in platformsList)
                {
                    using (var tran = db.GetTransaction())
                    {
                        if (platform.ID != 0)
                        {
                            db.DeleteWhere<PlatformSpeedRunComIDEntity>("PlatformID = @platformID", new { platformID = platform.ID });
                        }

                        db.Save<PlatformEntity>(platform);

                        var platformSpeedRunComID = new PlatformSpeedRunComIDEntity { PlatformID = platform.ID, SpeedRunComID = platform.SpeedRunComID };
                        db.Insert<PlatformSpeedRunComIDEntity>(platformSpeedRunComID);

                        tran.Complete();
                    }

                    _logger.Information("Saved Platforms {@Count} / {@Total}", count, platformsList.Count);
                    count++;
                }
            }
        }

        public IEnumerable<PlatformSpeedRunComIDEntity> GetPlatformSpeedRunComIDs(Expression<Func<PlatformSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<PlatformSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }
    }
}
