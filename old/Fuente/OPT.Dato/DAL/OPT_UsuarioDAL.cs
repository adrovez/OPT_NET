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
    public class OPT_UsuarioDAL
    {
        public SessionDTO Login(string pRut, string pClave)
        {
           
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;

                    return (from A in db.OPT_Usuario                           
                            where A.RutUsuario == pRut
                               && A.Clave == pClave                               
                            select new SessionDTO
                            {                                
                                Rut = A.RutUsuario,
                                Nombre = A.Nombre,
                                Mail = A.Mail,
                                idRol = A.OPT_Rol.idRol,
                                Celular = ""
                            }).SingleOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<OPT_UsuarioDTO> Listar(string pFiltro)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Usuario
                            where (A.Nombre.Contains(pFiltro) || pFiltro=="")
                            select new OPT_UsuarioDTO {

                                RutUsuario = A.RutUsuario,
                                Nombre = A.Nombre,
                                Mail = A.Mail,
                                Clave = A.Clave,
                                FechaIngreso = A.FechaIngreso,
                                Sucursal = (from B in db.OPT_Sucursal
                                            where  A.OPT_UsuarioSucursal.Select(x=>x.idSucursal).Contains(B.idSucursal)
                                            select B).ToList(),
                                idRol = A.OPT_Rol.idRol,
                                Rol = A.OPT_Rol.Rol

                                }).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public OPT_Usuario Buscar(string pRut)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    return (from A in db.OPT_Usuario
                                        .Include(x=>x.OPT_UsuarioSucursal)
                                        .Include(x=>x.OPT_Rol)
                            where A.RutUsuario == pRut
                            select A).SingleOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Insertar(OPT_Usuario pItem)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Usuario.Add(pItem);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void Editar(OPT_Usuario pItem)
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

        public void Eliminar(string pRutUsuario)
        {
            try
            {
                using (DB_OpticaEntities db = new DB_OpticaEntities())
                {
                    db.OPT_Usuario.Remove(db.OPT_Usuario.Where(x=>x.RutUsuario == pRutUsuario).SingleOrDefault());
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
