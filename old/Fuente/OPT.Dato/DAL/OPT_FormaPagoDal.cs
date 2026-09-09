using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
    public class OPT_FormaPagoDal
    {
        public List<OPT_FormaPago> Lista()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_FormaPago                           
                            orderby A.FormaPago
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
