using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OPT.Entidad;

namespace OPT.Dato.DAL
{
    public class OPT_ComunaDAL
    {
        public List<OPT_Comuna> Lista(int id)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;

                    return (from A in db.OPT_Comuna
                            where A.idRegion == id
                            orderby A.Comuna
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
