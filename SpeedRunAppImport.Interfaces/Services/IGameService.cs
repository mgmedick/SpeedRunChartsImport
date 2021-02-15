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
        void ProcessGames(DateTime lastImportDate, bool isFullImport, bool isBulkReload);
        List<Game> GetGamesWithRetry(int elementsPerPage, int elementsOffset, GameEmbeds embeds, GamesOrdering orderBy, int retryCount = 0);
        void SaveGames(IEnumerable<Game> games, bool isBulkReload);
    }
} 




