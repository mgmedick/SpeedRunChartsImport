using System;
using System.Collections.Generic;
using NPoco;
using System.Linq;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace SpeedRunAppImport.Repository
{
    public class SettingRespository : BaseRepository, ISettingRepository
    {
        public SettingRespository()
        {
        }

        public SettingEntity GetSetting(string name)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<SettingEntity>().Where(i => i.Name == name).FirstOrDefault();
            }
        }

        public void UpdateSetting(SettingEntity setting)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.Update(setting);
            }
        }
    } 
} 
