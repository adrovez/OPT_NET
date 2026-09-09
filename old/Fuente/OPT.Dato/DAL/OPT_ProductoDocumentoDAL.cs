using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
   public class OPT_ProductoDocumentoDAL
    {
        public List<OPT_ProductoDocumento> Lista(int idIngresoEgreso,string filtro)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_ProductoDocumento
                                        .Include(x=>x.OPT_TipoDocumento)
                            where A.IngresoEgreso == idIngresoEgreso
                              && (A.NumDocumento.Contains(filtro) || filtro=="")
                            orderby A.FechaDocumento
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_ProductoDocumento Buscar(long pId)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_ProductoDocumento
                                        .Include(x=>x.OPT_ProductoIngresoEgreso)                                        
                                        .Include(x => x.OPT_TipoDocumento)
                            where A.idProductoDocumento == pId
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }
                   
        public void Insertar(OPT_ProductoDocumento pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_ProductoDocumento.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_ProductoDocumento pItem)
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
