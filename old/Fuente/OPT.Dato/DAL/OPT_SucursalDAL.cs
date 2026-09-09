using OPT.Entidad;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


namespace OPT.Dato.DAL
{
    public class OPT_SucursalDAL
    {
        public List<OPT_Sucursal> BuscaAsignado(string pRutUsuario)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    var Item = (from A in db.OPT_Sucursal
                                join B in db.OPT_UsuarioSucursal on A.idSucursal equals B.idSucursal
                                where B.RutUsuario == pRutUsuario
                                orderby A.Nombre
                                select A).ToList();

                    return Item;

                }
            }
            catch
            {
                throw;
            }

        }

        public List<OPT_Sucursal> BuscaDisponible(string pRutUsuario)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    var Item = (from A in db.OPT_Sucursal
                                where !(from B in db.OPT_UsuarioSucursal
                                        where B.RutUsuario == pRutUsuario
                                        select B.idSucursal).Contains(A.idSucursal)
                                orderby A.Nombre
                                select A).ToList();

                    return Item;
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_Sucursal> ListaAll(string pFiltro)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Sucursal
                            where (A.Direccion.Contains(pFiltro) || pFiltro=="")
                               && (A.Nombre.Contains(pFiltro) || pFiltro == "")
                            orderby A.Nombre
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Sucursal Buscar(int pIdSucursal)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Sucursal.Include(x=>x.OPT_EmpresaSucursal)
                            where A.idSucursal == pIdSucursal
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Sucursal pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Sucursal.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Sucursal pItem)
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
