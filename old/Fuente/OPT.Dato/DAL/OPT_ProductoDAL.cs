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
    public class OPT_ProductoDAL
    {
        public List<OPT_Producto> Lista(string pProducto)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Producto
                            where (A.Producto.Contains(pProducto) || pProducto == "")                            
                            orderby A.Producto
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public List<OPT_Producto> Lista(List<long> id)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Producto
                            where (id.Contains(A.idProducto))
                            orderby A.Producto
                            select A).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Producto Buscar(long pIdProducto)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Producto
                            where A.idProducto == pIdProducto
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Producto BuscarPorCodigo(string  pCodigo)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Producto
                            where A.Codigo == pCodigo
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public bool ExisteCodigo(string pCodigo)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.OPT_Producto.Any(x => x.Codigo == pCodigo);
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Producto pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Producto.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Producto pItem)
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

        public SP_OTActualizaStock_Result ActualizaStock()
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return db.SP_OTActualizaStock().FirstOrDefault();

                }
            }
            catch
            {
                throw;
            }
        }
    }
}
