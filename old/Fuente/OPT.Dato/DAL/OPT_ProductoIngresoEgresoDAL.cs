using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
    public class OPT_ProductoIngresoEgresoDAL
    {
        public List<OPT_ProductoIngresoEgreso> Lista(long idProductoDocumento)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_ProductoIngresoEgreso
                                        .Include(x=>x.OPT_Producto)
                            where A.idProductoDocumento == idProductoDocumento
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_ProductoIngresoEgreso Buscar(long pId)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_ProductoIngresoEgreso
                                        .Include(x => x.OPT_Producto)
                            where A.idProductoIngreso == pId
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_ProductoIngresoEgreso pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_ProductoIngresoEgreso.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(List<OPT_ProductoIngresoEgreso> pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_ProductoIngresoEgreso.AddRange(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_ProductoIngresoEgreso pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.Entry(pItem).State = EntityState.Modified;
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
