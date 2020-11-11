using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface IPlatformService
    {
        IEnumerable<Platform> GetAllPlatforms();
    }
} 




