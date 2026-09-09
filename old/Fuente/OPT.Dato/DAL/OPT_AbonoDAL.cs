using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
    public class OPT_AbonoDAL
    {
        public List<OPT_Abono> Listar(long idOt)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.OPT_Abono.Where(x => x.idOT == idOt)
                                       .Include(x=>x.OPT_FormaPago)
                                       .ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Abono Buscar(long id)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                  return  db.OPT_Abono.Where(x=>x.idAbono == id)
                                      .Include(x => x.OPT_FormaPago)
                                      .SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Abono pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Abono.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Eliminar(OPT_Abono pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    var item = db.OPT_Abono.Where(x => x.idAbono == pItem.idAbono).FirstOrDefault();
                    db.OPT_Abono.Remove(item);
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
