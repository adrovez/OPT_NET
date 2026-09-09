using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace OPT.Web.Controllers
{
    [Authorize]
    public class LoginController : Controller
    {
        SessionDTO mySession;

        public LoginController()
        {
            mySession = new SessionDTO();
        }
        #region ====================== LOGIN USUARIO ======================

        [AllowAnonymous]
        [Libreria.NoLogin]
        public ActionResult Index()
        {
            try
            {
                if (Libreria.UsuarioConectado.GetExpiraSession() == false)
                {
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }

            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection myForm)
        {
            OPT_UsuarioDAL Datos = new OPT_UsuarioDAL();
            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();
            OPT_DefontanaDAL defontanaDAL = new OPT_DefontanaDAL();
            //EmailLoginRequest emailLoginRequest = new EmailLoginRequest();
            SessionDTO Item = new SessionDTO();
            bool vValida = false;
            string vRut;
            string vClave;

            try
            {
                //Cargo Modelo Usuario 
                vRut = myForm["username"].ToUpper().Trim();
                vClave = myForm["password"].Trim();

                //Valido el dato del usuario y cargo la session.
                Item = Datos.Login(vRut, vClave);

                if (Item != null)
                {
                    var sucursales = sucursalDAL.BuscaAsignado(vRut);
                    if (sucursales.Count() > 0)
                    {
                        Item.idSucursal = sucursales.First().idSucursal;

                        foreach (var sucursal in sucursales)
                        {
                            Item.Sucursal.Add(new OPT_Sucursal
                            {
                                idSucursal = sucursal.idSucursal,
                                Nombre = sucursal.Nombre,
                                Matriz = sucursal.Matriz,
                                Direccion = sucursal.Direccion
                            });
                        }
                    }

                    //Se cargan las propiedades en variables de session (esto lo realiza la clase)
                    Libreria.UsuarioConectado.SetMySession(Item);

                    //Crea la autentificacion.
                    FormsAuthentication.SetAuthCookie(Item.Rut.ToString(), false); //crea variable de usuario con el Rut del usuario                

                    vValida = true;
                }

                if (vValida)
                {
                    //Redirecciono al formulario Usuario                
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                else
                {
                    return View("Index"); //Retorno al formulario Login
                }

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return View("Index"); //Retorno al formulario Login
            }
        }

        #endregion

        public ActionResult LogOut()
        {
            //Ejecuto metodo ClearSession de la Clase MySession.
            Libreria.UsuarioConectado.ClearSession();
            //Retorno al formulario Login.
            return RedirectToAction("Index", "Login", new { area = "" });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public JsonResult Recuperar(FormCollection myForm)
        {
            OPT_UsuarioDAL Datos = new OPT_UsuarioDAL();
            OPT_Usuario Item = new OPT_Usuario();
            string vRut;
            string vAsunto = "Clave Acceso OPT";
            string vCuerpo = "Su clave de acceso para la plataforma OPT es: ";

            try
            {
                //Cargo Modelo Usuario 
                vRut = myForm["username"].ToUpper().Trim();

                //Valido el dato del usuario y cargo la session.
                Item = Datos.Buscar(vRut);

                if (Item == null)
                {
                    throw new Exception("Rut Ingresado no exite");
                }

                vCuerpo += Item.Clave;

                Libreria.EnvioMail mail = new Libreria.EnvioMail();

                mail.EnviarMail(vAsunto, vCuerpo, Item.Mail);

                return Json(new { ok = true, respuesta = Item.Mail }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}