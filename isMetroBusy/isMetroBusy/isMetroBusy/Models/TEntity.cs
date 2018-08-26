using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace isMetroBusy.Models
{
    public abstract class TEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
