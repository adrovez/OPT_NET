using OPT.Entidad;
using OPT.Entidad.DTO;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OPT.Dato.DAL
{
    public class OPT_OrdenDeTrabajoDAL
    {
        public List<OPT_OrdenDeTrabajo> ListaPendiente(int pIdEmpresa)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajo.Include(x => x.OPT_Pago)
                                                           .Include(x => x.OPT_Abono)
                            orderby A.FechaAtencion descending
                            where A.idEmpresa == pIdEmpresa 
                              &&  A.EstadoPago == "PENDIENTE"
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }


        public List<OPT_OrdenDeTrabajo> Lista(long pIdOT, int pIdEmpresa, string pRut, int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajo.Include(x => x.OPT_Cliente)
                                                           .Include(x => x.OPT_Empresa)
                                                           .Include(x => x.OPT_Sucursal)
                                                           .Include(x=>x.OPT_BitacoraOT)
                            orderby A.FechaAtencion descending
                            where (A.idOT == pIdOT || pIdOT == 0)
                              && (A.OPT_Cliente.RutCliente.Contains(pRut) || A.OPT_Cliente.Nombre.Contains(pRut))
                              && (A.idEmpresa == pIdEmpresa || pIdEmpresa == 0)
                              //&& A.FechaAtencion >= FechaDesde && A.FechaAtencion <= FechaHasta
                              && A.idSucursal == pIdSucursal
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_OrdenDeTrabajo> Lista(long pIdOT, int pIdEmpresa, string pRut, int pIdSucursal, int idEstado)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajo.Include(x => x.OPT_Cliente)
                                                           .Include(x => x.OPT_Empresa)
                                                           .Include(x => x.OPT_Sucursal)
                                                           .Include(x => x.OPT_RecetaCristales)
                            join B in db.OPT_BitacoraOT on A.idOT equals B.idOT
                            orderby A.FechaAtencion descending
                            where (A.idOT == pIdOT || pIdOT == 0)
                              && (A.OPT_Cliente.RutCliente.Contains(pRut) || A.OPT_Cliente.Nombre.Contains(pRut))
                              && (A.idEmpresa == pIdEmpresa || pIdEmpresa == 0)
                              && (B.Fecha == (A.OPT_BitacoraOT.Max(x => x.Fecha)) && B.idEstado == idEstado)
                              && A.idSucursal == pIdSucursal
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_OrdenDeTrabajo> Lista(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajo.Include(x => x.OPT_Cliente)
                                                            .Include(x => x.OPT_Empresa)                                                  
                                                            .Include(x => x.OPT_Sucursal)
                                                            .Include(x=>x.OPT_BitacoraOT)
                            orderby A.FechaAtencion descending
                            where A.RutCliente == pRut
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_OrdenDeTrabajo> ListaDeuda(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajo.Include(x => x.OPT_Cliente)
                                                            .Include(x => x.OPT_Empresa)
                                                            .Include(x => x.OPT_OrdenDeTrabajoDetalle)
                                                            .Include(x => x.OPT_Pago)
                                                            .Include(x => x.OPT_Cuota)
                                                            .Include(x => x.OPT_Sucursal)
                            orderby A.FechaAtencion descending
                            where A.RutCliente == pRut
                              && A.EstadoPago == "PENDIENTE"
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<sp_ListaDeudores_Result> ListaDeudores()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.sp_ListaDeudores().ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_OrdenDeTrabajo Buscar(long pIdOT)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajo.Include(x => x.OPT_Cliente)
                                                           .Include(x => x.OPT_Empresa)
                                                           .Include(x => x.OPT_Cliente.OPT_Comuna)
                                                           .Include(x => x.OPT_Cliente.OPT_Comuna.OPT_Region)
                                                           .Include(x => x.OPT_OrdenDeTrabajoDetalle)
                                                           .Include(x => x.OPT_Pago)
                                                           .Include(x => x.OPT_Cuota)
                                                           .Include(x => x.OPT_Sucursal)
                                                           .Include(x => x.OPT_Abono)                                                           
                                                           .Include(x => x.OPT_RecetaCristales)
                                                           .Include(x=> x.OPT_BitacoraOT)
                            where A.idOT == pIdOT
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OTPeriodoDTO> ListaPeriodo()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.sp_OTPeriodo()
                            select new OTPeriodoDTO
                            {
                                Periodo = A.Periodo.ToString(),
                                Precio = A.Precio.ToString(),
                                Abono = A.Abono.ToString(),
                                Saldo = A.Saldo.ToString(),
                                Cantidad = A.Cantidad.ToString(),
                                Empresa = A.Empresa
                            }).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OTPeriodoPieDTO> ListaPeriodoPie(int Anio)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.sp_OTPeriodo()
                            where A.Periodo == Anio
                            select new OTPeriodoPieDTO
                            {
                                value = string.Format(A.Precio.ToString(), "N0"),
                                label = A.Empresa.Replace("Ñ", "N").Replace("ñ", "n")

                            }).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OTPeriodoPieDTO> PeriodoMes(int Anio, int Mes, int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.sp_OTPeriodoMes(pIdSucursal)
                            where A.Anio == Anio
                               && A.Mes == Mes
                            select new OTPeriodoPieDTO
                            {
                                value = string.Format(A.Precio.ToString(), "N0"),
                                label = A.Empresa.Replace("Ñ", "N").Replace("ñ", "n")

                            }).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public decimal? SumaPeriodo(int Anio)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.sp_OTPeriodo()
                            where A.Periodo == Anio
                            select A.Precio).Sum();
                }
            }
            catch
            {
                throw;
            }
        }

        public decimal? SumaMes(int Anio, int Mes, int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.sp_OTPeriodoMes(pIdSucursal)
                            where A.Anio == Anio
                               && A.Mes == Mes
                            select A.Precio).Sum();
                }
            }
            catch
            {
                throw;
            }
        }

        public bool Existe(long pIdOT)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_OrdenDeTrabajo
                            where A.idOT == pIdOT
                            select A).Any();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_OrdenDeTrabajo pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_OrdenDeTrabajo.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_OrdenDeTrabajo pItem)
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

        public SP_OTEliminar_Result Eliminar(int id)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.SP_OTEliminar(id).FirstOrDefault();

                }
            }
            catch
            {
                throw;
            }
        }
    }
}
