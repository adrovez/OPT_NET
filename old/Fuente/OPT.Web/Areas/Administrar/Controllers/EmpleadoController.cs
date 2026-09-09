using Microsoft.Reporting.WebForms;
using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using System.Linq;

namespace OPT.Web.Areas.Administrar.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class EmpleadoController : Controller
    {
        SessionDTO mySession;

        public EmpleadoController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        #region ==================== Listar ====================

        // GET: Empresa/Empresa
        public ActionResult Index()
        {

            OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();

            int page = 1;
            int pageSize;

            try
            {
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());
                var Items = usuarioDAL.Listar("");
                return View(Items.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Paginar(FormCollection myForm)
        {
            OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();

            int page = 1;
            int pageSize;

            try
            {
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());
                if (Request.QueryString["pPageNumber"] != null)
                {
                    page = int.Parse(Request.QueryString["pPageNumber"]);
                }
                var pFiltro = myForm["pFiltro"].ToString();

                var Lista = usuarioDAL.Listar(pFiltro);

                return PartialView("_ParcialBusqueda", Lista.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }


        }

        [HttpGet]
        public void Reporte()
        {
            //string mimeType = string.Empty;
            //string encoding = string.Empty;
            //string extension = string.Empty;
            //Warning[] warnings;
            //string[] streams;

            //Ds.dsClienteListaTableAdapters.rpt_ClienteListaTableAdapter ds = new Ds.dsClienteListaTableAdapters.rpt_ClienteListaTableAdapter();

            //ReportViewer reportViewer1 = new ReportViewer();
            //ReportDataSource rds = new ReportDataSource();

            //try
            //{
            //    rds.Name = "dsClienteLista";
            //    rds.Value = ds.GetData();

            //    reportViewer1.LocalReport.DataSources.Clear();
            //    reportViewer1.LocalReport.DataSources.Add(rds);

            //    reportViewer1.LocalReport.ReportPath = "Areas/Clientes/Rpt/rptClienteLista.rdlc";
            //    reportViewer1.LocalReport.Refresh();

            //    //reportViewer1.RefreshReport();
            //    byte[] bytes = reportViewer1.LocalReport.Render("Excel"
            //                                                   , null
            //                                                   , out mimeType
            //                                                   , out encoding
            //                                                   , out extension
            //                                                   , out streams
            //                                                   , out warnings);

            //    var NomArchivo = "Clientes_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

            //    Response.Clear();
            //    Response.ClearContent();
            //    Response.ClearHeaders();
            //    Response.BufferOutput = true;
            //    Response.ContentType = "application/octet-stream";
            //    Response.AddHeader("Content-Disposition", "attachment; filename=" + NomArchivo);

            //    Response.BinaryWrite(bytes);                            // crea el archivo....
            //    Response.Flush();

            //}
            //catch (Exception ex)
            //{
            //    Libreria.LogError.setLogExeption(ex);
            //}
        }

        [HttpGet]
        public PartialViewResult Details(string id)
        {

            OPT_ClienteDAL Datos = new OPT_ClienteDAL();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();
                OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();

                var Item = usuarioDAL.Buscar(id);

                ViewBag.SucursalAsignado = new SelectList(sucursalDAL.BuscaAsignado(Item.RutUsuario), "idSucursal", "Nombre");

                return PartialView("_ModalDetails", Item);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        #endregion

        #region ==================== CREATE ====================

        [HttpGet]
        public PartialViewResult Create()
        {
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }


                OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();
                OPT_RolDAL rolDAL = new OPT_RolDAL();

                ViewBag.ComboSucursal = new SelectList(sucursalDAL.ListaAll(""), "idSucursal", "Nombre");
                ViewBag.ComboRol = new SelectList(rolDAL.ListaAll(), "idRol", "Rol");

                return PartialView("_ModalCreate");

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection myForm)
        {
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();
                OPT_UsuarioSucuasalDAL usuarioSucuasalDAL = new OPT_UsuarioSucuasalDAL();

                OPT_Usuario _Usuario = new OPT_Usuario();
                List<OPT_UsuarioSucursal> _UsuarioSucursals = new List<OPT_UsuarioSucursal>();

                string[] ArraySucursal = myForm["idSucursal"].Split(',');

                _Usuario.RutUsuario = myForm["Rut"].Replace(".", "").Trim().ToUpper();
                _Usuario.Nombre = myForm["Nombre"].Trim().ToUpper();
                _Usuario.Mail = myForm["Mail"].Trim().ToUpper();
                _Usuario.FechaIngreso = DateTime.Now;
                _Usuario.idRol = int.Parse(myForm["idRol"]);
                _Usuario.Clave = myForm["Clave"].Trim();


                var ExisteRut = usuarioDAL.Buscar(_Usuario.RutUsuario);

                if (ExisteRut != null)
                {
                    throw new Exception("Rut ingresado ya esta registrado");
                }

                usuarioDAL.Insertar(_Usuario);

                #region ==================== SUCURSAL EMPLEADO ====================
                foreach (var x in ArraySucursal)
                {
                    _UsuarioSucursals.Add(new OPT_UsuarioSucursal
                    {
                        idSucursal = int.Parse(x),
                        RutUsuario = _Usuario.RutUsuario
                    });
                }

                usuarioSucuasalDAL.Insertar(_UsuarioSucursals);
                #endregion


                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region ==================== EDITAR ====================

        [HttpGet]
        public ActionResult Edit(string id)
        {

            OPT_ClienteDAL Datos = new OPT_ClienteDAL();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();
                OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();
                OPT_RolDAL rolDAL = new OPT_RolDAL();
                List<SelectListItem> comboSucursal = new List<SelectListItem>();

                var Item = usuarioDAL.Buscar(id);

                foreach (var x in sucursalDAL.ListaAll(""))
                {
                    comboSucursal.Add(new SelectListItem
                    {
                        Value = x.idSucursal.ToString(),
                        Text = x.Nombre.ToString(),
                        Selected = Item.OPT_UsuarioSucursal.Any(a => a.idSucursal == x.idSucursal)
                    });
                }

                ViewBag.ComboSucursal = comboSucursal;
                ViewBag.ComboRol = new SelectList(rolDAL.ListaAll(), "idRol", "Rol", Item.idRol);

                return PartialView("_ModalEdit", Item);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection myForm)
        {
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();
                OPT_UsuarioSucuasalDAL usuarioSucuasalDAL = new OPT_UsuarioSucuasalDAL();

                OPT_Usuario _Usuario = new OPT_Usuario();
                List<OPT_UsuarioSucursal> _UsuarioSucursals = new List<OPT_UsuarioSucursal>();

                string[] ArraySucursal = myForm["idSucursal"].Split(',');

                _Usuario = usuarioDAL.Buscar(myForm["Rut"].Replace(".", "").Trim().ToUpper());
                _Usuario.Nombre = myForm["Nombre"].Trim().ToUpper();
                _Usuario.Mail = myForm["Mail"].Trim().ToUpper();
                _Usuario.idRol = int.Parse(myForm["idRol"]);
                _Usuario.Clave = myForm["Clave"].Trim();

                usuarioDAL.Editar(_Usuario);

                #region ==================== SUCURSAL EMPLEADO ====================
                foreach (var x in ArraySucursal)
                {
                    _UsuarioSucursals.Add(new OPT_UsuarioSucursal
                    {
                        idSucursal = int.Parse(x),
                        RutUsuario = _Usuario.RutUsuario
                    });
                }

                usuarioSucuasalDAL.Eliminar(_Usuario.RutUsuario);
                usuarioSucuasalDAL.Insertar(_UsuarioSucursals);
                #endregion


                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion

        #region ==================== ELIMINAR ====================

        [HttpPost]
        public JsonResult Eliminar(string id)
        {

            OPT_ClienteDAL Datos = new OPT_ClienteDAL();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                OPT_UsuarioDAL usuarioDAL = new OPT_UsuarioDAL();               
                OPT_UsuarioSucuasalDAL usuarioSucuasalDAL = new OPT_UsuarioSucuasalDAL();

                var Item = usuarioDAL.Buscar(id);

                usuarioSucuasalDAL.Eliminar(Item.RutUsuario);
                usuarioDAL.Eliminar(Item.RutUsuario);

                return Json(new {ok=true,respuesta="" },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

    }
}