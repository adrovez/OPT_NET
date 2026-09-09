using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.Data.Entity.Validation;
using OPT.Entidad;


namespace OPT.Dato.DAL
{
    public class OPT_OrdenDeTrabajoDetalleDAL
    {
        public List<OPT_OrdenDeTrabajoDetalle> Lista(long pIdOT)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajoDetalle.Include(x => x.OPT_Producto)
                            where A.idOT == pIdOT
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_OrdenDeTrabajoDetalle Buscar(long pIdOTDetalle)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajoDetalle.Include(x => x.OPT_Producto)
                            where A.idOTDetalle == pIdOTDetalle
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_OrdenDeTrabajoDetalle pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_OrdenDeTrabajoDetalle.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void InsertarAll(List<OPT_OrdenDeTrabajoDetalle> pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_OrdenDeTrabajoDetalle.AddRange(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_OrdenDeTrabajoDetalle pItem)
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

        public void Eliminar(long pId)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    var OldItem = db.OPT_OrdenDeTrabajoDetalle.Where(x => x.idOTDetalle == pId).SingleOrDefault();
                    db.OPT_OrdenDeTrabajoDetalle.Remove(OldItem);
                    db.SaveChanges();
                }
            }           
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }
    }

}
