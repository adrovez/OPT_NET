using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
    public class OPT_RecetaCristalesDAL
    {
        public OPT_RecetaCristales Buscar(long pId)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {                  
                    return (from A in db.OPT_RecetaCristales.Include(x=>x.OPT_OrdenDeTrabajo)
                            where A.idRecetaCristales == pId
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_RecetaCristales BuscarById(long pId)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    return (from A in db.OPT_RecetaCristales
                            where A.idRecetaCristales == pId
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_RecetaCristales Buscar(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    return (from A in db.OPT_RecetaCristales
                            where A.RutCliente == pRut
                               && A.idOT== null
                            orderby A.FechaIngreso descending
                            select A).FirstOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_RecetaCristales> Listar(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_RecetaCristales
                                        .Include(x => x.OPT_OrdenDeTrabajo)
                            where A.RutCliente == pRut
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_RecetaCristales pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_RecetaCristales.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_RecetaCristales pItem)
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
