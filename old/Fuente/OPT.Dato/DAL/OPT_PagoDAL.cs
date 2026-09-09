using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using OPT.Entidad;

namespace OPT.Dato.DAL
{
    public class OPT_PagoDAL
    {
        public List<OPT_Pago> Lista(long pIdOT)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Pago
                            orderby A.FechaPago descending
                            where A.idOT == pIdOT
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_Pago> Lista(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Pago.Include(x => x.OPT_OrdenDeTrabajo)
                            orderby A.FechaPago descending
                            where A.OPT_OrdenDeTrabajo.RutCliente == pRut
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_Pago> ListaDeuda(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Pago.Include(x => x.OPT_OrdenDeTrabajo)
                                //.Include(x => x.OPT_Pago)
                            orderby A.FechaPago descending
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

        public List<sp_DetalleDeudores_Result> DetalleDeudores(int pIdEmpresa)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.sp_DetalleDeudores(pIdEmpresa).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Pago Buscar(long pIdPago)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Pago
                            where A.idPago == pIdPago
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Pago pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Pago.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Pago pItem)
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
