using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
    public class OPT_TipoDocumentoDAL
    {
        public List<OPT_TipoDocumento> Lista(int idTipo)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_TipoDocumento                                       
                            where A.IngresoEgreso == idTipo
                            orderby A.TipoDocumento
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
