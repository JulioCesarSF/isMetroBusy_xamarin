using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using HtmlAgilityPack;
using isMetroBusy.DAL.DAO.LinhaFavoritaDAO;
using isMetroBusy.DAL.DAO.SettingDAO;
using isMetroBusy.Models;
using isMetroBusy.ViewModels.DTO;
using MetroSPHelper.Metro;
using MetroSPHelper.Models;
using Plugin.LocalNotifications;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace isMetroBusy.ViewModels
{
    public class LinhasViewModel : BaseViewModel
    {
        #region PRIVATE
        private LinhaFavoritaDAOImpl _dao = new LinhaFavoritaDAOImpl();
        private SettingDAOImpl _daoSettings = new SettingDAOImpl();
        private ObservableCollection<LinhaStatus> _linhaStatus;
        private string _status;
        private LinhaStatus _selectedRow;
        private bool _isRefreshing = false;
        private bool _started = false;
        private List<LinhaStatus> _linhaStatusOld;
        private string _oldCPTM;
        private DiretoDoMetro _oldDiretoDoMetro;
        //AutoUpdate
        private int _refreshListInSeconds = 0;
        private DateTime _lastAutoUpdateTime;
        public CancellationTokenSource _cancelTask = new CancellationTokenSource();
        #endregion

        #region PUBLIC
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; NotifyPropetyChanged(); }
        }
        public LinhaStatus SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (value is LinhaStatus)
                {
                    var newStatus = _dao.AddOrRemove(value);
                    _selectedRow = newStatus;
                    NotifyPropetyChanged();
                    _selectedRow = null;
                    UpdateLinhasAsync(shouldRequest: false);
                }
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                NotifyPropetyChanged();
            }
        }
        public ObservableCollection<LinhaStatus> LinhaStatus
        {
            get => _linhaStatus;
            set
            {
                _linhaStatus = value;
                NotifyPropetyChanged();
            }
        }
        #endregion

        public LinhasViewModel()
        {
            Task.Factory.StartNew(() => UpdateLinhasAsync());
            //StartAutoUpdate();
        }

        #region PUBLIC methods
        public void StartAutoUpdate()
        {
            var autoUpdate = _daoSettings.GetSettingValueByName("AutoUpdate");
            if (autoUpdate is Setting)
            {
                if (!autoUpdate.Valor.Equals("OFF"))
                {
                    int valor = 0;
                    int.TryParse(autoUpdate.Valor, out valor);
                    _refreshListInSeconds = valor;
                }
                else
                {
                    _refreshListInSeconds = 0;
                }
            }

            _lastAutoUpdateTime = DateTime.Now;
            Task.Factory.StartNew(() => AutoUpdateAsync(), _cancelTask.Token);
        }
        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (!IsRefreshing)
                    {
                        var task = await UpdateLinhasAsync();
                        if (!task)
                        {
                            Status = "Status: Wait a few seconds to try again.";
                        }
                    }
                    else
                    {
                        Status = "Status: Wait a few seconds to try again.";
                    }

                    IsRefreshing = false;
                });
            }
        }
        #endregion

        #region PRIVATE methods
        private async Task AutoUpdateAsync()
        {
            try
            {
                if (!_started)
                {
                    _started = true;
                    while (!_cancelTask.IsCancellationRequested)
                    {
                        var shouldUpdate = _refreshListInSeconds > 0 &&
                                           (int) (DateTime.Now - _lastAutoUpdateTime).TotalSeconds >
                                           _refreshListInSeconds &&
                                           !IsRefreshing;
                        if (shouldUpdate)
                        {
                            _lastAutoUpdateTime = DateTime.Now;
                            await UpdateLinhasAsync();
                        }

                        await Task.Delay(100);
                    }

                    _started = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _started = false;
            }

            if (!_started)
            {
                StartAutoUpdate();
            }
        }
        private async Task<bool> UpdateLinhasAsync(bool isRefreshing = true, bool shouldRequest = true)
        {
            try
            {
                IsRefreshing = isRefreshing;
                Status = "Status: updating...";
                if (shouldRequest)
                {
                    var metro = await Metro.GetSituacaoMetroAsync();
                    _oldDiretoDoMetro = metro;
                    var cptm = await Metro.GetSituacaCPTMAsync();
                    _oldCPTM = cptm;
                    var all = MergeSituacao(metro.Linhas.Linha, cptm);
                    if (all.Count > 0)
                    {
                        _linhaStatusOld = LinhaStatus?.ToList();
                        LinhaStatus = ProcessLinhaStatus(all);
                    }
                }
                else
                {
                    var newAll = MergeSituacao(_oldDiretoDoMetro.Linhas.Linha, _oldCPTM);
                    var newList = ProcessLinhaStatus(newAll);
                    LinhaStatus = null;
                    LinhaStatus = newList;
                }

                Status = $"Last Update: {DateTime.Now} - (Auto: {_refreshListInSeconds > 0})";
                IsRefreshing = false;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Status = "Status: update failed.";
            }
            IsRefreshing = false;
            return false;
        }
        private ObservableCollection<LinhaStatus> ProcessLinhaStatus(List<Linha> linhas)
        {
            var newList = new ObservableCollection<LinhaStatus>();
            var favorites = _dao.GetAll();
            int countFavorites = 0;
            linhas.ForEach(_ =>
            {
                var isFavorite = favorites.FirstOrDefault(f => f.Nome.Equals(_.Nome)) != null;
                if (isFavorite) countFavorites++;
                newList.Add(new LinhaStatus
                {
                    Nome = _.Nome,
                    Status = _.Situacao,
                    Favorite = isFavorite
                });
            });

            if (countFavorites > 0)
            {
                ShowNotification(newList);
            }

            return newList;
        }
        private void ShowNotification(ObservableCollection<LinhaStatus> newList)
        {
            var showNotification = _daoSettings.GetSettingValueByName("Notification");
            if (!showNotification.Valor.Equals("1")) return;
            var list = newList;
            int countChangesForFavorite = 0;
            var sb = new StringBuilder();
            list?.ForEach(n =>
            {
                _linhaStatusOld?.ForEach(o =>
                {
                    if (o.Favorite && o.Nome.Equals(n.Nome) && !o.Status.Equals(n.Status))
                    {
                        countChangesForFavorite++;
                        sb.Append($"{n.Nome}-{n.Status}\n");
                    }
                });
            });

            if (countChangesForFavorite > 0)
            {
                CrossLocalNotifications.Current.Show("New status", $"{sb.ToString()}");
            }
        }
        private List<Linha> MergeSituacao(List<Linha> metro, string htmlCPTM)
        {
            Status = $"Status: merging requests";
            if (metro is null)
            {
                metro = new List<Linha>();
                Status = $"Status: try again";
            }
            var newList = metro;
            if (!string.IsNullOrEmpty(htmlCPTM))
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlCPTM);
                var linhasCPTM = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, 'nome_linha')]");
                var statusCPTM = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, 'status')]");
                for (var i = 0; i < linhasCPTM.Count; i++)
                {
                    var newLinha = new Linha
                    {
                        Nome = GetCPTMNome(linhasCPTM[i].InnerHtml),
                        Situacao = statusCPTM[i].InnerHtml
                    };
                    var exists = newList.FirstOrDefault(_ => _.Nome.Equals(newLinha.Nome));
                    if (exists is null)
                    {
                        newList.Add(newLinha);
                    }
                }
            }

            return newList;
        }
        private string GetCPTMNome(string innerHtml)
        {
            if (string.IsNullOrEmpty(innerHtml))
            {
                return "";
            }

            switch (innerHtml)
            {
                case "RUBI":
                    return "Linha 7-Rubi";
                case "DIAMANTE":
                    return "Linha 8-Diamante";
                case "ESMERALDA":
                    return "Linha 9-Esmeralda";
                case "TURQUESA":
                    return "Linha 10-Turquesa";
                case "CORAL":
                    return "Linha 11-Coral";
                case "SAFIRA":
                    return "Linha 12-Safira";
                case "JADE":
                    return "Linha 13-Jade";
                default:
                    return innerHtml;
            }
        }
        #endregion
    }
}
