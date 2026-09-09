using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using OPT.Entidad;

namespace OPT.Dato.DAL
{
    public class OPT_CuotaDAL
    {
        public List<OPT_Cuota> Lista(long pIdOT)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Cuota
                            orderby A.FechaBencimiento descending
                            where A.idOT == pIdOT
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_Cuota> Lista(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Cuota.Include(x => x.OPT_OrdenDeTrabajo)
                            orderby A.FechaBencimiento descending
                            where A.OPT_OrdenDeTrabajo.RutCliente == pRut
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_Cuota> ListaDeuda(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Cuota.Include(x => x.OPT_OrdenDeTrabajo)
                                //.Include(x => x.OPT_Pago)
                            orderby A.FechaBencimiento descending
                            where A.OPT_OrdenDeTrabajo.RutCliente == pRut
                              && A.OPT_OrdenDeTrabajo.EstadoPago == "PENDIENTE"
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Cuota Buscar(long pIdCuota)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Cuota
                            where A.idCuota == pIdCuota
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Cuota pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Cuota.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(List<OPT_Cuota> pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Cuota.AddRange(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Cuota pItem)
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

        public void Eliminar(long pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Cuota.RemoveRange(db.OPT_Cuota.Where(x=>x.idOT == pItem));
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
