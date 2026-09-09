using Microsoft.Reporting.WebForms;
using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using PagedList;
using System;
using System.Configuration;
using System.Web.Mvc;


namespace OPT.Web.Areas.Administrar.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class MisDatosController : Controller
    {
        
        SessionDTO mySession;

        public MisDatosController()
        {           
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        // GET: Usuarios/Usuario/Edit/5
        public ActionResult Index()
        {
            OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();
            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();

            try
            {
                var Item = usuarioDAL.Buscar(mySession.Rut);
                ViewBag.SucursalAsignado = new SelectList(sucursalDAL.BuscaAsignado(Item.RutUsuario), "idSucursal", "Nombre");

                return View(Item);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        // POST: Usuarios/Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Index(OPT_Usuario item)
        {
            OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                var ItemUsuario = usuarioDAL.Buscar(mySession.Rut);

                ItemUsuario.Clave = item.Clave.Trim();
                ItemUsuario.Nombre = item.Nombre;
                ItemUsuario.Mail = item.Mail;

                usuarioDAL.Editar(ItemUsuario);

                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}