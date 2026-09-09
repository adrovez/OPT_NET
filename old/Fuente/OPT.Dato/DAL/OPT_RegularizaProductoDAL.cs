using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
    public class OPT_RegularizaProductoDAL
    {
        public List<OPT_RegularizaProducto> Lista(long idProducto)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_RegularizaProducto
                            where A.idProducto == idProducto
                            orderby A.Fecha
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_RegularizaProducto pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_RegularizaProducto.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
