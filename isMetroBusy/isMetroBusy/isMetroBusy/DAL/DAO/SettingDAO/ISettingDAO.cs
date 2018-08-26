using System;
using System.Collections.Generic;
using System.Text;
using isMetroBusy.Models;

namespace isMetroBusy.DAL.DAO.SettingDAO
{
    public interface ISettingDAO : IGenericDAO<Setting>
    {
        Setting GetSettingValueByName(string settingName);
        bool UpdateByName(string settingName, string valor);
    }
}
