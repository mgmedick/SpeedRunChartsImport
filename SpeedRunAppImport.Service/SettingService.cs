using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace SpeedRunAppImport.Service
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepo = null;

        public SettingService(ISettingRepository settingRepo)
        {
            _settingRepo = settingRepo;
        }

        public SettingEntity GetSetting(string name)
        {
            return _settingRepo.GetSetting(name);
        }

        public void UpdateSetting(SettingEntity setting)
        {
            _settingRepo.UpdateSetting(setting);
        }
    }
}


