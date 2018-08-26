using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using isMetroBusy.Models;
using isMetroBusy.ViewModels.DTO;

namespace isMetroBusy.DAL.DAO.LinhaFavoritaDAO
{
    public class LinhaFavoritaDAOImpl : ILinhaFavoritaDAO, IDisposable
    {
        private SQLiteContext<LinhaFavorita> _db;
        public LinhaFavoritaDAOImpl()
        {
            _db = new SQLiteContext<LinhaFavorita>();
        }
        public LinhaFavorita Add(LinhaFavorita entity, bool checkExists = true)
        {
            try
            {
                using (var con = _db.Connection)
                {
                    if (checkExists)
                    {
                        var exists = con.Find<LinhaFavorita>(_ => _.Nome.Equals(entity.Nome));
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
                Console.WriteLine(e);
                //throw;
            }

            return null;
        }

        public LinhaStatus AddOrRemove(LinhaStatus linhaStatus, bool firstTime = false)
        {
            try
            {
                using (var con = _db.Connection)
                {
                    var linhaFavorita = con.Find<LinhaFavorita>(_ => _.Nome.Equals(linhaStatus.Nome));
                    if (linhaFavorita is LinhaFavorita)
                    {
                        Delete(linhaFavorita);
                        linhaStatus.Favorite = false;
                        return linhaStatus;
                    }
                    else if(!firstTime)
                    {
                        var newFavorite = Add(new LinhaFavorita
                        {
                            Nome = linhaStatus.Nome
                        });
                        linhaStatus.Favorite = true;
                        return linhaStatus;
                    }

                    return linhaStatus;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool Delete(LinhaFavorita entity)
        {
            try
            {
                using (var con = _db.Connection)
                {
                    var linhaFavorita = con.Find<LinhaFavorita>(_ => _.Nome.Equals(entity.Nome));
                    if (linhaFavorita is LinhaFavorita)
                    {
                        entity.Id = linhaFavorita.Id;
                        return con.Delete(entity) > 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return false;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public List<LinhaFavorita> GetAll()
        {
            try
            {
                using (var con = _db.Connection)
                {
                    return con.Table<LinhaFavorita>().ToList() ?? new List<LinhaFavorita>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool Update(LinhaFavorita entity)
        {
            try
            {
                using (var con = _db.Connection)
                {
                    var linhaFavorita =
                        con.Query<LinhaFavorita>("SELECT * FROM LinhaFavorita WHERE Nome like '?'", entity.Nome).FirstOrDefault();
                    if (linhaFavorita is LinhaFavorita)
                    {
                        entity.Id = linhaFavorita.Id;
                        return con.Update(entity) > 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return false;
        }
    }
}
