using OPT.Entidad;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OPT.Dato.DAL
{
    public class OPT_EmpresaDAL
    {
        public List<OPT_Empresa> Lista(string pEmpresa,int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Empresa
                            join B in db.OPT_EmpresaSucursal on A.idEmpresa equals B.idEmpresa                           
                            where (A.Empresa.Contains(pEmpresa) || pEmpresa == "")
                            && B.idSucursal == pIdSucursal
                            orderby A.Empresa
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_Empresa> ComboEmpresa(int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Empresa
                            join B in db.OPT_EmpresaSucursal on A.idEmpresa equals B.idEmpresa
                            where B.idSucursal == pIdSucursal
                            orderby A.Empresa
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }

        }

        public List<OPT_Empresa> ComboEmpresaAll()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Empresa
                            orderby A.Empresa
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }

        }


        public OPT_Empresa Buscar(int pIdEmpresa)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Empresa
                            where A.idEmpresa == pIdEmpresa
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Empresa pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Empresa.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Empresa pItem)
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
