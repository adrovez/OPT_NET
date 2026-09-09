using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OPT.Dato.DAL
{
    public class OPT_AtencionDAL
    {
        public List<OPT_Atencion> Lista(DateTime fecha)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Atencion
                                        .Include(x => x.OPT_Cliente)
                            where (A.FechaAtencion>= fecha)
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }
        
        public OPT_Atencion Buscar(long Id)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Atencion
                                        .Include(x => x.OPT_Cliente)
                            where A.idAtencion == Id
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Atencion BuscarByIdOt(long Id)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Atencion
                            where A.idOT == Id
                            select A).FirstOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Atencion pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Atencion.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Atencion pItem)
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
