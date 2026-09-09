using OPT.Entidad;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OPT.Dato.DAL
{
    public class OPT_UsuarioSucuasalDAL
    {
        public void Insertar(List<OPT_UsuarioSucursal> Item)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                  
                    db.OPT_UsuarioSucursal.AddRange(Item);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }

        }

        public void Editar(OPT_UsuarioSucursal Item)
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

        public void Eliminar(string pRutUsuario)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_UsuarioSucursal.RemoveRange(db.OPT_UsuarioSucursal.Where(x => x.RutUsuario == pRutUsuario).ToList());
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
