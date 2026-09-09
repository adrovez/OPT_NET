using OPT.Dato.DAL;
using OPT.Entidad.DTO;
using System;
using System.Web.Mvc;

namespace OPT.Web.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class UtilController : Controller
    {
        SessionDTO mySession;

        public UtilController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        [HttpPost]
        public PartialViewResult Menu()
        {
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                return PartialView("_ParcialMenu");
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw;
            }
        }

        [HttpPost]
        public JsonResult DatosSession()
        {
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                return Json(new { ok = true, respuesta = mySession }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Comuna(int id)
        {
            OPT_ComunaDAL DatoComuna = new OPT_ComunaDAL();
            try
            {
                var Lista = DatoComuna.Lista(id);

                return Json(new { ok = true, respuesta = Lista }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ExisteRut(string id)
        {
            OPT_ClienteDAL DatoCliente = new OPT_ClienteDAL();

            try
            {
                var Existe = DatoCliente.Existe(id.Replace(".", ""));

                if (Existe == true)
                {
                    throw new Exception("Rut ya esta tegistrado.");
                }

                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public JsonResult SelectSucursal(int id)
        {
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                mySession.idSucursal = id;
                Libreria.UsuarioConectado.SetMySession(mySession);

                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public int CalcularEdad(DateTime fechaNacimiento)
        {
            // Obtiene la fecha actual:
            DateTime fechaActual = DateTime.Today;

            // Comprueba que la se haya introducido una fecha válida; si 
            // la fecha de nacimiento es mayor a la fecha actual se muestra mensaje 
            // de advertencia:
            if (fechaNacimiento > fechaActual)
            {                
                return -1;
            }
            else
            {
                int edad = fechaActual.Year - fechaNacimiento.Year;

                // Comprueba que el mes de la fecha de nacimiento es mayor 
                // que el mes de la fecha actual:
                if (fechaNacimiento.Month > fechaActual.Month)
                {
                    --edad;
                }
                return edad;
            }
        }
    }
}