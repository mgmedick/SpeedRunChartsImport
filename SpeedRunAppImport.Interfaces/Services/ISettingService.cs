using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface ISettingService
    {
        SettingEntity GetSetting(string name);
        void UpdateSetting(SettingEntity setting);
    }
}





