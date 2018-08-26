using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace isMetroBusy.Models
{
    public class Setting : TEntity
    {
        [Unique]
        public string Nome { get; set; }
        public string Valor { get; set; }
        public bool Selected { get; set; }
    }
}
