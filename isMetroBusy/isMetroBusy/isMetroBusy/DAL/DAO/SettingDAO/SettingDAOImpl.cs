using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using isMetroBusy.Models;

namespace isMetroBusy.DAL.DAO.SettingDAO
{
    public class SettingDAOImpl : ISettingDAO, IDisposable
    {
        private SQLiteContext<Setting> _db;

        public SettingDAOImpl()
        {
            _db = new SQLiteContext<Setting>();
            Seed();
        }

        private void Seed()
        {
            using (var con = _db.Connection)
            {
                var autoUpdate = new List<Setting>
                {
                    new Setting
                    {
                        Nome = "AutoUpdate",
                        Valor = "OFF",
                        Selected = true
                    },
                    new Setting
                    {
                        Nome = "Notification",
                        Valor = "0"
                    }
                };

                try
                {
                    con.InsertAll(autoUpdate);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //throw;
                }
            }
        }

        public Setting Add(Setting entity, bool checkExists = true)
        {
            if (entity is null)
            {
                return null;
            }
            try
            {
                using (var con = _db.Connection)
                {
                    if (checkExists)
                    {
                        var exists = con.Find<Setting>(_ => _.Nome.Equals(entity.Nome));
                        if (exists is null)
                        {
                            if (con.Insert(entity) > 0)
                            {
                                return entity;
                            }
                        }
                        else
                        {
                            return exists;
                        }
                    }
                    else
                    {
                        if (con.Insert(entity) > 0)
                        {
                            return entity;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //throw;
            }

            return null;
        }

        public bool Delete(Setting entity)
        {
            try
            {
                using (var con = _db.Connection)
                {
                    return con.Delete(entity) > 0;
                }
            }
            catch (Exception e)
            {
                //throw;
            }

            return false;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public List<Setting> GetAll()
        {
            try
            {
                using (var con = _db.Connection)
                {
                    return con.Table<Setting>().ToList();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public Setting GetSettingValueByName(string settingName)
        {
            try
            {
                using (var con = _db.Connection)
                {
                    return con.Find<Setting>(_ => _.Nome.Equals(settingName));
                }
            }
            catch (Exception e)
            {
                //throw;
            }

            return null;
        }

        public bool Update(Setting entity)
        {
            try
            {
                using (var con = _db.Connection)
                {
                    if (entity is Setting)
                    {
                        entity.Selected = true;
                    }
                    else return false;
                    var reset = con.Query<Setting>("UPDATE Setting SET Selected=?", 0);
                    var result = con.Query<Setting>("UPDATE Setting SET Selected=? WHERE Nome=? AND Valor=?",
                        entity.Selected ? 1 : 0, entity.Nome, entity.Valor);
                    return result.Count > 0;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public bool UpdateByName(string settingName, string valor)
        {
            try
            {
                if (string.IsNullOrEmpty(settingName)) return false;

                using (var con = _db.Connection)
                {
                    var exists = con.Find<Setting>(_ => _.Nome.Equals(settingName));
                    if (exists is Setting)
                    {
                        exists.Valor = valor;
                        return con.Update(exists) > 0;
                    }
                    else
                    {
                        Add(new Setting
                        {
                            Nome = settingName,
                            Valor = valor
                        }, false);
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }

            return false;
        }
    }
}
