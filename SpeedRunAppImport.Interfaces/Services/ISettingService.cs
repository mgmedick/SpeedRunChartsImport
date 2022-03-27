using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface ISettingService
    {
        SettingEntity GetSetting(string name);
        void UpdateSetting(string name, string value);
        void UpdateSetting(string name, DateTime value);
        void UpdateSetting(string name, int value);
        void UpdateSetting(SettingEntity setting);
        string GetTwitchToken();
        bool GenerateAndMoveSitemapXml();
    }
}





