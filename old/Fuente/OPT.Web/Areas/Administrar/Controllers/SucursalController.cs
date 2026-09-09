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
    public class SucursalController : Controller
    {
        SessionDTO mySession;

        public SucursalController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        #region ==================== Listar ====================

        // GET: Empresa/Empresa
        public ActionResult Index()
        {
            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();

            int page = 1;
            int pageSize;

            try
            {
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());

                var Lista = sucursalDAL.ListaAll("");
                return View(Lista.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Index(FormCollection myForm)
        {
            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();

            int page = 1;
            int pageSize;

            try
            {
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());
                if (Request.QueryString["pPageNumber"] != null)
                {
                    page = int.Parse(Request.QueryString["pPageNumber"]);
                }

                string pFiltro = myForm["Filtro"].ToString();
                var Lista = sucursalDAL.ListaAll(pFiltro);

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
        public PartialViewResult Details(int id)
        {
            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                var Item = sucursalDAL.Buscar(id);

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
        public ActionResult Create()
        {
            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();
            OPT_EmpresaDAL empresaDAL = new OPT_EmpresaDAL();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }
                ViewBag.ComboEmpresa = new SelectList(empresaDAL.ComboEmpresaAll(), "idEmpresa", "Empresa");
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
        public JsonResult Create(FormCollection myForm)
        {
            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();
            OPT_EmpresaSucursalDAL empresaSucursalDAL = new OPT_EmpresaSucursalDAL();
            OPT_Sucursal _Sucursal = new OPT_Sucursal();
            List<OPT_EmpresaSucursal> empresaSucursal = new List<OPT_EmpresaSucursal>();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }


                _Sucursal.Nombre = myForm["Nombre"].Trim();
                _Sucursal.Telefono = myForm["Telefono"].Trim();
                _Sucursal.FechaRegistro = DateTime.Now;
                _Sucursal.Matriz = bool.Parse(myForm["Matriz"].Trim());
                _Sucursal.Direccion = myForm["Direccion"].Trim();

                sucursalDAL.Insertar(_Sucursal);

                string[] ArrayEmpresa = myForm["idEmpresa"].Split(',');

                #region ==================== EMPRESA SUCURSAL ====================
                foreach (var x in ArrayEmpresa)
                {
                    empresaSucursal.Add(new OPT_EmpresaSucursal
                    {
                        idEmpresa = int.Parse(x),
                        idSucursal = _Sucursal.idSucursal
                    });
                }

                empresaSucursalDAL.Insertar(empresaSucursal);
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
        public ActionResult Edit(int id)
        {

            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();
            OPT_EmpresaDAL empresaDAL = new OPT_EmpresaDAL();
            List<SelectListItem> comboEmpresa = new List<SelectListItem>();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                var Item = sucursalDAL.Buscar(id);


                foreach (var x in empresaDAL.ComboEmpresaAll())
                {
                    comboEmpresa.Add(new SelectListItem
                    {
                        Value = x.idEmpresa.ToString(),
                        Text = x.Empresa.ToString(),
                        Selected = Item.OPT_EmpresaSucursal.Any(a => a.idEmpresa == x.idEmpresa)
                    });
                }

                ViewBag.ComboEmpresa = comboEmpresa;

                return PartialView("_ModalEdit", Item);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(FormCollection myForm)
        {
            OPT_SucursalDAL sucursalDAL = new OPT_SucursalDAL();
            OPT_EmpresaSucursalDAL empresaSucursalDAL = new OPT_EmpresaSucursalDAL();
            OPT_Sucursal _Sucursal = new OPT_Sucursal();
            List<OPT_EmpresaSucursal> _empresaSucursal = new List<OPT_EmpresaSucursal>();

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    throw new Exception("Erro al enviar datos.");
                }

                _Sucursal = sucursalDAL.Buscar(int.Parse(myForm["idSucursal"].Trim()));
                _empresaSucursal = empresaSucursalDAL.BuscarBySucursal(int.Parse(myForm["idSucursal"].Trim()));

                #region ==================== EDITAR SUCURSAL ====================
                _Sucursal.Nombre = myForm["Nombre"].Trim();
                _Sucursal.Telefono = myForm["Telefono"].Trim();
                _Sucursal.Matriz = bool.Parse(myForm["Matriz"].Trim());
                _Sucursal.Direccion = myForm["Direccion"].Trim();

                sucursalDAL.Editar(_Sucursal);
                #endregion

                string[] ArrayEmpresa = myForm["idEmpresa"].Split(',');

                #region ==================== EMPRESA SUCURSAL ====================
                empresaSucursalDAL.Eliminar(_Sucursal.idSucursal);
                _empresaSucursal = new List<OPT_EmpresaSucursal>();

                foreach (var x in ArrayEmpresa)
                {
                    _empresaSucursal.Add(new OPT_EmpresaSucursal
                    {
                        idEmpresa = int.Parse(x),
                        idSucursal = _Sucursal.idSucursal
                    });
                }

                empresaSucursalDAL.Insertar(_empresaSucursal);
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
    }

}