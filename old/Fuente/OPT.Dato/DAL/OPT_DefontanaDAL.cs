using OPT.Entidad;
using System.Linq;

namespace OPT.Dato.DAL
{
    public class OPT_DefontanaDAL
    {
        public OPT_Defontana Buscar()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.OPT_Defontana.FirstOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
