using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface ISettingRepository
    {
        SettingEntity GetSetting(string name);
        void UpdateSetting(SettingEntity setting);
    }
}





