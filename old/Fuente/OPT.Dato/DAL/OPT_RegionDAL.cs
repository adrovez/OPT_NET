using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OPT.Entidad;


namespace OPT.Dato.DAL
{
    public class OPT_RegionDAL
    {
        public List<OPT_Region> Lista()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Region
                            orderby A.Region
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
