using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface IGameService
    {
        IEnumerable<Game> GetGames(DateTime lastImportDate, bool isFullImport);
    }
} 




