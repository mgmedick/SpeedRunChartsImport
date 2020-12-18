using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
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

        public void UpdateSetting(string name, DateTime value)
        {
            var setting = _settingRepo.GetSetting(name);
            setting.Dte = value;
            _settingRepo.UpdateSetting(setting);
        }

        public void UpdateSetting(string name, string value)
        {
            var setting = _settingRepo.GetSetting(name);
            setting.Str = value;
            _settingRepo.UpdateSetting(setting);
        }

        public void UpdateSetting(string name, int value)
        {
            var setting = _settingRepo.GetSetting(name);
            setting.Num = value;
            _settingRepo.UpdateSetting(setting);
        }

        public void UpdateSetting(SettingEntity setting)
        {
            _settingRepo.UpdateSetting(setting);
        }

    }
}


