using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface IGameService
    {
        bool ProcessGames(DateTime lastImportDate, bool isFullImport, bool isBulkReload);
        List<Game> GetGamesWithRetry(int elementsPerPage, ref int elementsOffset, GameEmbeds embeds, GamesOrdering? orderBy, int retryCount = 0, int badRecordRetryCount = 0, int? initElementsPerPage = null);
        void SaveGames(IEnumerable<Game> games, bool isBulkReload);
    }
} 




