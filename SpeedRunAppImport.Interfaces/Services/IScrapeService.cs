using SpeedRunAppImport.Model.Entity;
using System.Collections.Generic;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface IScrapeService
    {
        IEnumerable<string> GetLatestSpeedRunIDs();
    }
}
