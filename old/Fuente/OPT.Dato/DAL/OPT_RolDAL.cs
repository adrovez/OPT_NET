using OPT.Entidad;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OPT.Dato.DAL
{
    public class OPT_RolDAL
    {       
        public List<OPT_Rol> ListaAll()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Rol                           
                            orderby A.Rol
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
