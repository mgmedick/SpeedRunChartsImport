using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using NPoco.Extensions;
using System.Linq;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Repository
{
    public class SpeedRunRespository : BaseRepository, ISpeedRunRepository
    {
        private readonly ILogger _logger;

        public SpeedRunRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void InsertSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunGuestEntity> guests, IEnumerable<SpeedRunVideoEntity> videos, IEnumerable<SpeedRunVideoDetailEntity> videoDetails)
        {
            _logger.Information("Started InsertSpeedRuns");
            int batchCount = 0;
            var speedRunsList = speedRuns.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                while (batchCount < speedRunsList.Count)
                {
                    var runsBatch = speedRunsList.Skip(batchCount).Take(MaxBulkRows).ToList();
                    var runSpeedRunComIDs = runsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var speedRunLinksBatch = speedRunLinks.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                    var speedRunSystemsBatch = speedRunSystems.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                    var speedRunTimesBatch = speedRunTimes.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                    var speedRunCommentsBatch = speedRunComments.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                    var variableValuesBatch = variableValues.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                    var playersBatch = players.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                    var guestsBatch = guests.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                    var videosBatch = videos.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                    var videosLocalIDBatch = videosBatch.Select(i => i.LocalID).ToList();
                    var videoDetailsBatch = videoDetails.Where(i => videosLocalIDBatch.Contains(i.SpeedRunVideoLocalID)).ToList();

                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBatch<SpeedRunEntity>(runsBatch);
                        var runIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_SpeedRun_Full ORDER BY ID DESC LIMIT @0;", runsBatch.Count).Reverse().ToArray() :
                                               db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_SpeedRun_Full ORDER BY ID DESC", runsBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < runsBatch.Count; i++)
                        {
                            runsBatch[i].ID = runIDs[i];
                        }

                        var speedRunSpeedRunComIDsBatch = runsBatch.Select(i => new SpeedRunSpeedRunComIDEntity { SpeedRunID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<SpeedRunSpeedRunComIDEntity>(speedRunSpeedRunComIDsBatch);

                        speedRunLinksBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBatch<SpeedRunLinkEntity>(speedRunLinksBatch);

                        speedRunSystemsBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBatch<SpeedRunSystemEntity>(speedRunSystemsBatch);

                        speedRunTimesBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBatch<SpeedRunTimeEntity>(speedRunTimesBatch);

                        speedRunCommentsBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBatch<SpeedRunCommentEntity>(speedRunCommentsBatch);

                        variableValuesBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBatch<SpeedRunVariableValueEntity>(variableValuesBatch);

                        playersBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBatch<SpeedRunPlayerEntity>(playersBatch);

                        guestsBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBatch<SpeedRunGuestEntity>(guestsBatch);

                        videosBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBatch<SpeedRunVideoEntity>(videosBatch);

                        var videoIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_SpeedRun_Video_Full ORDER BY ID DESC LIMIT @0;", videosBatch.Count).Reverse().ToArray() :
                                                 db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_SpeedRun_Video_Full ORDER BY ID DESC", videosBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < videosBatch.Count; i++)
                        {
                            videosBatch[i].ID = videoIDs[i];
                        }

                        foreach (var videoDetail in videoDetailsBatch)
                        {
                            var video = videosBatch.Find(g => g.LocalID == videoDetail.SpeedRunVideoLocalID);
                            videoDetail.SpeedRunVideoID = video.ID;
                            videoDetail.SpeedRunID = video.SpeedRunID;
                        }

                        db.InsertBatch<SpeedRunVideoDetailEntity>(videoDetailsBatch);

                        tran.Complete();
                    }

                    _logger.Information("Saved speedRuns {@Count} / {@Total}", runsBatch.Count, speedRunsList.Count);
                    batchCount += MaxBulkRows;
                }
            }
            _logger.Information("Completed InsertSpeedRuns");
        }

        public void SaveSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunGuestEntity> guests, IEnumerable<SpeedRunVideoEntity> videos, IEnumerable<SpeedRunVideoDetailEntity> videoDetails)
        {
            _logger.Information("Started SaveSpeedRuns");
            int count = 1;
            var speedRunsList = speedRuns.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                foreach (var speedRun in speedRuns)
                {
                    var speedRunLink = speedRunLinks.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                    var speedRunSystem = speedRunSystems.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                    var speedRunTime = speedRunTimes.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                    var speedRunComment = speedRunComments.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                    var variableValuesBatch = variableValues.Where(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID).ToList();
                    var playersBatch = players.Where(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID).ToList();
                    var guestsBatch = guests.Where(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID).ToList();
                    var videosBatch = videos.Where(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID).ToList();
                    var videosLocalIDBatch = videosBatch.Select(i => i.LocalID).ToList();
                    var videoDetailsBatch = videoDetails.Where(i => videosLocalIDBatch.Contains(i.SpeedRunVideoLocalID)).ToList();

                    using (var tran = db.GetTransaction())
                    {
                        try
                        {
                            if (speedRun.ID != 0)
                            {
                                //_logger.Information("Deleting secondary run entities");
                                speedRun.ModifiedDate = DateTime.UtcNow;
                                db.DeleteWhere<SpeedRunLinkEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                                db.DeleteWhere<SpeedRunSystemEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                                db.DeleteWhere<SpeedRunTimeEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                                db.DeleteWhere<SpeedRunCommentEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                                db.DeleteWhere<SpeedRunVariableValueEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                                db.DeleteWhere<SpeedRunPlayerEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                                db.DeleteWhere<SpeedRunGuestEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                                db.DeleteWhere<SpeedRunVideoEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                                db.DeleteWhere<SpeedRunVideoDetailEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            }

                            //_logger.Information("Saving run");
                            db.Save<SpeedRunEntity>(speedRun);
                            db.Save<SpeedRunSpeedRunComIDEntity>(new SpeedRunSpeedRunComIDEntity { SpeedRunID = speedRun.ID, SpeedRunComID = speedRun.SpeedRunComID });

                            if (speedRunLink != null)
                            {
                                //_logger.Information("Saving run link");
                                speedRunLink.SpeedRunID = speedRun.ID;
                                db.Insert<SpeedRunLinkEntity>(speedRunLink);
                            }

                            if (speedRunSystem != null)
                            {
                                //_logger.Information("Saving run system");
                                speedRunSystem.SpeedRunID = speedRun.ID;
                                db.Insert<SpeedRunSystemEntity>(speedRunSystem);
                            }

                            if (speedRunTime != null)
                            {
                                //_logger.Information("Saving run time");
                                speedRunTime.SpeedRunID = speedRun.ID;
                                db.Insert<SpeedRunTimeEntity>(speedRunTime);
                            }

                            if (speedRunComment != null)
                            {
                                //_logger.Information("Saving run comment");
                                speedRunComment.SpeedRunID = speedRun.ID;
                                db.Insert<SpeedRunCommentEntity>(speedRunComment);
                            }

                            //_logger.Information("Saving run variableValues");
                            variableValuesBatch.ForEach(i => i.SpeedRunID = speedRun.ID);
                            db.InsertBatch<SpeedRunVariableValueEntity>(variableValuesBatch);

                            //_logger.Information("Saving run players");
                            playersBatch.ForEach(i => i.SpeedRunID = speedRun.ID);
                            db.InsertBatch<SpeedRunPlayerEntity>(playersBatch);

                            //_logger.Information("Saving run guests");
                            guestsBatch.ForEach(i => i.SpeedRunID = speedRun.ID);
                            db.InsertBatch<SpeedRunGuestEntity>(guestsBatch);

                            //_logger.Information("Saving run videos");
                            foreach (var video in videosBatch)
                            {
                                video.SpeedRunID = speedRun.ID;
                                db.Insert<SpeedRunVideoEntity>(video);
                            }

                            foreach (var videoDetail in videoDetailsBatch)
                            {
                                var video = videosBatch.Find(g => g.LocalID == videoDetail.SpeedRunVideoLocalID);
                                videoDetail.SpeedRunVideoID = video.ID;
                                videoDetail.SpeedRunID = video.SpeedRunID;
                            }

                            db.InsertBatch<SpeedRunVideoDetailEntity>(videoDetailsBatch);

                            tran.Complete();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "SaveSpeedRuns SpeedRunID: {@SpeedRunID}, SpeedRunSpeedRunComID: {@SpeedRunSpeedRunComID}", speedRun.ID, speedRun.SpeedRunComID);
                        }
                    }

                    _logger.Information("Saved speedRuns {@Count} / {@Total}", count, speedRunsList.Count);
                    count++;
                }
            }

            _logger.Information("Completed SaveSpeedRuns");
        }

        public void InsertSpeedRunVideoDetails(IEnumerable<SpeedRunVideoDetailEntity> videoDetails)
        {
            _logger.Information("Started InsertSpeedRunVideoDetails");
            int batchCount = 0;
            var videoDetailsList = videoDetails.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                while (batchCount < videoDetailsList.Count)
                {
                    var videoDetailsBatch = videoDetailsList.Skip(batchCount).Take(MaxBulkRows).ToList();

                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBatch<SpeedRunVideoDetailEntity>(videoDetailsBatch);

                        tran.Complete();
                    }

                    _logger.Information("Saved videoDetails {@Count} / {@Total}", videoDetailsBatch.Count, videoDetailsList.Count);
                    batchCount += MaxBulkRows;
                }
            }

            _logger.Information("Completed InsertSpeedRunVideoDetails");
        }

        public void SaveSpeedRunVideoDetails(IEnumerable<SpeedRunVideoDetailEntity> videoDetails)
        {
            _logger.Information("Started SaveSpeedRunVideoDetails");
            int count = 1;
            var videoDetailsList = videoDetails.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                foreach (var videoDetail in videoDetails)
                {
                    try
                    {
                        db.Save<SpeedRunVideoDetailEntity>(videoDetail);
                        _logger.Information("Saved videoDetail {@Count} / {@Total}", count, videoDetailsList.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "SaveSpeedRunVideoDetails SpeedRunVideoID: {@SpeedRunVideoID}, SpeedRunID: {@SpeedRunID}", videoDetail.SpeedRunVideoID, videoDetail.SpeedRunID);
                    }

                    count++;
                }
            }

            _logger.Information("Completed SaveSpeedRunVideoDetails");
        }

        public void DeleteSpeedRuns(string predicate)
        {
            _logger.Information("Started DeleteSpeedRuns");
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                var speedRunIDsToDelete = db.Query<SpeedRunEntity>().WhereSql(predicate).ToList().Select(i => i.ID).ToList();
                _logger.Information("Found {@Count} SpeedRuns to Delete", speedRunIDsToDelete.Count());
                DeleteSpeedRunsByID(speedRunIDsToDelete);
                _logger.Information("Completed DeleteSpeedRuns");
            }
        }

        public void DeleteSpeedRuns(Expression<Func<SpeedRunEntity, bool>> predicate)
        {
            _logger.Information("Started DeleteSpeedRuns");
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                var speedRunIDsToDelete = db.Query<SpeedRunEntity>().Where(predicate).ToList().Select(i => i.ID).ToList();
                _logger.Information("Found {@Count} SpeedRuns to Delete", speedRunIDsToDelete.Count());
                DeleteSpeedRunsByID(speedRunIDsToDelete);
                _logger.Information("Completed DeleteSpeedRuns");
            }
        }

        public void DeleteSpeedRunsByID(IEnumerable<int> speedRunIDsToDelete)
        {
            _logger.Information("Started DeleteSpeedRunsByID");
            int count = 1;

            using (IDatabase db = DBFactory.GetDatabase())
            {
                foreach (var speedRunID in speedRunIDsToDelete)
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunLinkEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunSystemEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunTimeEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunCommentEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunVariableValueEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunPlayerEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunGuestEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunVideoEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunVideoDetailEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunSpeedRunComIDEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRunID });
                        db.OneTimeCommandTimeout = 32767;
                        db.DeleteWhere<SpeedRunEntity>("ID = @speedRunID", new { speedRunID = speedRunID });

                        tran.Complete();
                    }

                    _logger.Information("Deleted speedRuns {@Count} / {@Total}", count, speedRunIDsToDelete.Count());
                    count++;
                }
            }

            _logger.Information("Completed DeleteSpeedRunsByID");
        }

        public IEnumerable<SpeedRunSpeedRunComIDEntity> GetSpeedRunSpeedRunComIDs(Expression<Func<SpeedRunSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<SpeedRunSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<SpeedRunVideoEntity> GetSpeedRunVideos(Expression<Func<SpeedRunVideoEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<SpeedRunVideoEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public bool CreateFullTables()
        {
            bool result = true;

            try
            {
                _logger.Information("Started CreateFullTables");
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        if (IsMySQL)
                        {
                            db.Execute("CALL ImportCreateFullTables;");
                        }
                        else
                        {
                            db.Execute("EXEC dbo.ImportCreateFullTables");
                        }
                        tran.Complete();
                    }
                }
                _logger.Information("Completed CreateFullTables");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "CreateFullTables");
            }

            return result;
        }

        public bool ReorderSpeedRuns()
        {
            bool result = true;

            try
            {
                _logger.Information("Started ReorderSpeedRuns");
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.OneTimeCommandTimeout = 32767;
                        if (IsMySQL)
                        {
                            db.Execute("CALL ImportReorderSpeedRuns;");
                        }
                        else
                        {
                            db.Execute("EXEC dbo.ImportReorderSpeedRuns");
                        }
                        tran.Complete();
                    }
                }
                _logger.Information("Completed ReorderSpeedRuns");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "ReorderSpeedRuns");
            }

            return result;
        }

        public bool RenameFullTables()
        {
            bool result = true;

            try
            {
                _logger.Information("Started RenameFullTables");
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.OneTimeCommandTimeout = 32767;
                        if (IsMySQL)
                        {
                            db.Execute("CALL ImportRenameFullTables;");
                        }
                        else
                        {
                            db.Execute("EXEC dbo.ImportRenameFullTables");
                        }
                        tran.Complete();
                    }
                }
                _logger.Information("Completed RenameFullTables");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "RenameFullTables");
            }

            return result;
        }

        public bool UpdateSpeedRunRanksFull()
        {
            bool result = true;

            try
            {
                _logger.Information("Started UpdateSpeedRunRanksFull");
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    db.OneTimeCommandTimeout = 32767;
                    if (IsMySQL)
                    {
                        db.Execute("CALL ImportUpdateSpeedRunRanksFull;");
                    }
                    else
                    {
                        db.Execute("EXEC dbo.ImportUpdateSpeedRunRanksFull");
                    }
                }
                _logger.Information("Completed UpdateSpeedRunRanksFull");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "UpdateSpeedRunRanksFull");
            }

            return result;
        }

        public bool UpdateSpeedRunRanks(DateTime lastImportDateUtc)
        {
            bool result = true;

            try
            {
                _logger.Information("Started UpdateSpeedRunRanks {@LastImportDateUtc}", lastImportDateUtc);
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    db.OneTimeCommandTimeout = 32767;
                    if (IsMySQL)
                    {
                        db.Execute("CALL ImportUpdateSpeedRunRanks (@0);", lastImportDateUtc);
                    }
                    else
                    {
                        db.Execute("EXEC dbo.ImportUpdateSpeedRunRanks @0", lastImportDateUtc);
                    }
                }
                _logger.Information("Completed UpdateSpeedRunRanks");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "UpdateSpeedRunRanks");
            }

            return result;
        }

        public bool RebuildIndexes()
        {
            bool result = true;

            try
            {
                _logger.Information("Started RebuildIndexes");
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    db.OneTimeCommandTimeout = 32767;
                    if (IsMySQL)
                    {
                        db.Execute("CALL ImportRebuildIndexes;");
                    }
                    else
                    {
                        db.Execute("EXEC dbo.ImportRebuildIndexes");
                    }
                }
                _logger.Information("Completed RebuildIndexes");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "RebuildIndexes");
            }

            return result;
        }

        public bool UpdateStats()
        {
            bool result = true;

            try
            {
                _logger.Information("Started UpdateStats");
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    db.OneTimeCommandTimeout = 32767;
                    if (!IsMySQL)
                    {
                        db.Execute("EXEC sp_updatestats");
                    }
                }
                _logger.Information("Completed UpdateStats");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "UpdateStats");
            }

            return result;
        }
    }
}
