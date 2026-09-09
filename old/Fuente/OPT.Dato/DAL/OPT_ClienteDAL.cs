using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using OPT.Entidad;
using OPT.Entidad.DTO;

namespace OPT.Dato.DAL
{
    public class OPT_ClienteDAL
    {
        public List<OPT_ClienteDTO> Lista(int pIdEmpresa, string pRut, string pNombre, int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Cliente
                            join B in db.OPT_EmpresaSucursal on A.idEmpresa equals B.idEmpresa
                            orderby A.idEmpresa, A.Nombre
                            where (A.idEmpresa == pIdEmpresa || pIdEmpresa == 0)
                              && (A.RutCliente.Contains(pRut) || pRut == "")
                              && (A.Nombre.Contains(pNombre) || pNombre == "")
                              && (B.idSucursal == pIdSucursal)

                            select new OPT_ClienteDTO
                            {
                                RutCliente = A.RutCliente,
                                idEmpresa = A.idEmpresa,
                                Empresa = A.OPT_Empresa.Empresa,
                                Nombre = A.Nombre,
                                Direccion = A.Direccion,
                                idComuna = A.OPT_Comuna.idComuna,
                                Comuna = A.OPT_Comuna.Comuna,
                                idRegion = A.OPT_Comuna.OPT_Region.idRegion,
                                Region = A.OPT_Comuna.OPT_Region.Region,
                                Celular = A.Celular,
                                Mail = A.Mail,
                                FechaIngreso = A.FechaIngreso,
                                FechaNacimiento = A.FechaNacimiento
                            }).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Cliente Buscar(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Cliente.Include(x => x.OPT_Comuna)
                                                    .Include(x => x.OPT_Comuna.OPT_Region)
                                                    .Include(x => x.OPT_Empresa)
                            where A.RutCliente == pRut
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Cliente BuscarEditar(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Cliente
                            where A.RutCliente == pRut
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public bool Existe(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Cliente
                            where A.RutCliente == pRut
                            select A).Any();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Cliente pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Cliente.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Cliente pItem)
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
