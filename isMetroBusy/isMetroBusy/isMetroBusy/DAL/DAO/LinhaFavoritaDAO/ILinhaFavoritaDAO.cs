using System;
using System.Collections.Generic;
using System.Text;
using isMetroBusy.Models;
using isMetroBusy.ViewModels.DTO;

namespace isMetroBusy.DAL.DAO.LinhaFavoritaDAO
{
    public interface ILinhaFavoritaDAO : IGenericDAO<LinhaFavorita>
    {
        LinhaStatus AddOrRemove(LinhaStatus linhaStatus, bool firstTime = false);
    }
}
