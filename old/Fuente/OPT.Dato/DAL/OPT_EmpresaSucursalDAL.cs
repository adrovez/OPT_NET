using OPT.Entidad;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OPT.Dato.DAL
{
    public class OPT_EmpresaSucursalDAL
    {
        public bool Existe(int pIdEmresa, int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {

                    return db.OPT_EmpresaSucursal.Any(x => x.idEmpresa == pIdEmresa && x.idSucursal == pIdSucursal);                    
                }
            }
            catch
            {
                throw;
            }

        }

        public List<OPT_EmpresaSucursal> BuscarBySucursal( int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                   return db.OPT_EmpresaSucursal.Where(x => x.idSucursal == pIdSucursal).ToList();   
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(List<OPT_EmpresaSucursal> Item)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {

                    db.OPT_EmpresaSucursal.AddRange(Item);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }

        }

        public void Editar(OPT_EmpresaSucursal Item)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.Entry(Item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Eliminar(int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_EmpresaSucursal.RemoveRange(db.OPT_EmpresaSucursal.Where(x => x.idSucursal == pIdSucursal).ToList());
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
