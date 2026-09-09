using OPT.Entidad;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OPT.Dato.DAL
{
    public class OPT_BitacoraOtDAL
    {
        public List<OPT_BitacoraOT> Listar(int idOt)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.OPT_BitacoraOT.Where(x => x.idOT == idOt).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_BitacoraOT pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_BitacoraOT.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(List<OPT_BitacoraOT> pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_BitacoraOT.AddRange(pItem);
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
