using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using isMetroBusy.DAL.DAO.SettingDAO;
using isMetroBusy.Models;

namespace isMetroBusy.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region PRIVATE
        private SettingDAOImpl _dao = new SettingDAOImpl();
        private List<string> _autoUpdateSetting;
        private string _selectedAutoUpdate;
        private bool _isNotificationEnabled;
        #endregion

        #region PUBLIC
        public bool IsNotificationEnabled
        {
            get => _isNotificationEnabled;
            set
            {
                if (value != _isNotificationEnabled)
                {
                    _isNotificationEnabled = value;
                    NotifyPropetyChanged();
                    _dao.UpdateByName("Notification", _isNotificationEnabled ? "1" : "0");
                }
            }
        }
        public string SelectedAutoUpdate
        {
            get => _selectedAutoUpdate;
            set
            {
                if (value != _selectedAutoUpdate)
                {
                    _selectedAutoUpdate = value;
                    NotifyPropetyChanged();
                    _dao.UpdateByName("AutoUpdate", _selectedAutoUpdate);
                }
            }
        }
        public List<string> AutoUpdateSetting
        {
            get => _autoUpdateSetting;
            set
            {
                _autoUpdateSetting = value;
                NotifyPropetyChanged();
            }
        }
        #endregion

        public SettingsViewModel()
        {
            Setup();
        }

        private void Setup()
        {
            SetupAutoUpdate();
            SetupNotification();
        }

        private void SetupAutoUpdate()
        {
            AutoUpdateSetting = new List<string>
            {
                "OFF", "120", "240", "480"
            };

            var current = _dao.GetSettingValueByName("AutoUpdate");
            if (current is Setting)
            {
                SelectedAutoUpdate = current.Valor;
            }
            else
            {
                SelectedAutoUpdate = "OFF";
            }
        }

        private void SetupNotification()
        {
            var current = _dao.GetSettingValueByName("Notification");
            if (current is Setting)
            {
                if (current.Valor.Equals("1"))
                {
                    IsNotificationEnabled = true;
                }
                else IsNotificationEnabled = false;
            }
            else
            {
                IsNotificationEnabled = false;
            }
        }
    }
}
