using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SQLite;

namespace isMetroBusy.DAL
{
    public class SQLiteContext<T> : IDisposable where T : class
    {
        private string _dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "jsMetro.db");
        public SQLiteConnection Connection {
            get { return new SQLiteConnection(_dbFile); }
        }
        public SQLiteContext()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                Connection.CreateTable<T>();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (Connection is SQLiteConnection)
                {
                    Connection.Close();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
