using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
    public class OPT_EstadoOtDAL
    {
        public List<OPT_EstadoOT> Listar()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.OPT_EstadoOT.ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_EstadoOT Buscar(int Id)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.OPT_EstadoOT.Where(x => x.idEstadoOT == Id).FirstOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
