using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPT.Dato.DAL
{
    public class OPT_AnamnesisDAL
    {
        public List<OPT_Anamnesis> Lista(string RutCliente, int idEmpresa, int idSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Anamnesis
                                        .Include(x => x.OPT_Cliente)
                                        .Include(x => x.OPT_Sucursal)
                                        .Include(x => x.OPT_Empresa)
                            where (A.RutCliente == RutCliente || RutCliente == "")
                              && (A.idEmpresa == idEmpresa || idEmpresa == 0)
                              && A.idSucursal == idSucursal
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_Anamnesis> ListaPorRut(string RutCliente)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Anamnesis
                                        .Include(x => x.OPT_Cliente)
                                        .Include(x => x.OPT_Sucursal)
                                        .Include(x => x.OPT_Empresa)
                            where A.RutCliente == RutCliente                             
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Anamnesis Buscar(long Id)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Anamnesis
                                        .Include(x => x.OPT_Cliente)
                                        .Include(x => x.OPT_Cliente.OPT_Comuna)
                                        .Include(x => x.OPT_Cliente.OPT_Comuna.OPT_Region)
                                        .Include(x => x.OPT_Empresa)
                            where A.idAnamnesis == Id
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Anamnesis pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Anamnesis.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Anamnesis pItem)
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
