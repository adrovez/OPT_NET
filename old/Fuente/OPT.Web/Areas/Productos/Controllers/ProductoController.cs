using Microsoft.Reporting.WebForms;
using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using OPT.Web.Areas.Productos.Models;
using OPT.Web.Models;
using PagedList;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace OPT.Web.Areas.Productos.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class ProductoController : Controller
    {
        SessionDTO mySession;

        public ProductoController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        #region ==================== LISTAR ====================
        // GET: Empresa/Empresa
        public ActionResult Index()
        {

            OPT_ProductoDAL Datos = new OPT_ProductoDAL();
            int page = 1;
            int pageSize;

            try
            {
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());
                var Lista = Datos.Lista("");

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
        public PartialViewResult Paginar(FormCollection myForm)
        {
            OPT_ProductoDAL Datos = new OPT_ProductoDAL();

            int page = 1;
            int pageSize;

            try
            {
                pageSize = int.Parse(ConfigurationManager.AppSettings["Paginar"].ToString());
                if (Request.QueryString["pPageNumber"] != null)
                {
                    page = int.Parse(Request.QueryString["pPageNumber"]);
                }

                var pProducto = myForm["Producto"].ToString();

                var Lista = Datos.Lista(pProducto);

                return PartialView("_ParcialListaProducto", Lista.ToPagedList(page, pageSize));
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
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(OPT_Producto Item)
        {
            OPT_ProductoDAL Datos = new OPT_ProductoDAL();
            JsonResponse<int> response = new JsonResponse<int>();
            try
            {
                Item.Codigo = Item.Codigo.ToUpper();
                Item.Producto = Item.Producto.ToUpper();
                Item.Stock = 0;
                Item.Entrada = 0;
                Item.Salida = 0;
                Item.FechaIngreso = DateTime.Now;

                Datos.Insertar(Item);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                response.ok = false;
                response.Mensaje = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Existe(string id)
        {
            OPT_ProductoDAL Datos = new OPT_ProductoDAL();
            JsonResponse<int> response = new JsonResponse<int>();
            try
            {
                var ItemProducto = Datos.ExisteCodigo(id.ToUpper().Trim());

                if (ItemProducto == true)
                {
                    response.ok = false;
                    response.Mensaje = "Codigo de producto ya fue ingresado.";
                }
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                response.ok = false;
                response.Mensaje = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region ==================== EDITAR ====================

        [HttpGet]
        public ActionResult Edit(long id)
        {
            OPT_ProductoDAL Dato = new OPT_ProductoDAL();

            try
            {
                var Item = Dato.Buscar(id);
                return View(Item);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OPT_Producto Item)
        {
            OPT_ProductoDAL Datos = new OPT_ProductoDAL();
            try
            {
                var ItemProducto = Datos.Buscar(Item.idProducto);
                ItemProducto.Producto = Item.Producto.ToUpper();
                ItemProducto.StockMinimo = Item.StockMinimo;
                ItemProducto.StockMaximo = Item.StockMaximo;
                ItemProducto.ControlStock = Item.ControlStock;

                Datos.Editar(ItemProducto);

                return RedirectToAction("Index", "Producto");
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }

        }

        #endregion


        #region ==================== Regularizar ====================

        [HttpGet]
        public ActionResult Regularizar(long id)
        {
            OPT_RegularizaProductoDAL regularizaProductoDAL = new OPT_RegularizaProductoDAL();
            OPT_ProductoDAL productoDAL = new OPT_ProductoDAL();
            RegularizaProductoResponse response = new RegularizaProductoResponse();

            try
            {
                response.regularizaProducto = regularizaProductoDAL.Lista(id);
                response.producto = productoDAL.Buscar(id);

                return View(response);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Regularizar(OPT_RegularizaProducto Item)
        {
            OPT_ProductoDAL productoDAL = new OPT_ProductoDAL();
            OPT_RegularizaProductoDAL regularizaProductoDAL = new OPT_RegularizaProductoDAL();
            try
            {
                var ItemProducto = productoDAL.Buscar(Item.idProducto);

                Item.StockAnterior = int.Parse(ItemProducto.Stock.ToString());
                Item.Fecha = DateTime.Now;
                Item.Responsable = mySession.Nombre;
                regularizaProductoDAL.Insertar(Item);

                ItemProducto.Stock = Item.StockNuevo;
                productoDAL.Editar(ItemProducto);              

                return RedirectToAction("Index", "Producto");
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }

        }

        #endregion

        [HttpGet]
        public void Reporte()
        {
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;
            Warning[] warnings;
            string[] streams;

            Ds.dsProductoListaTableAdapters.OPT_ProductoTableAdapter ds = new Ds.dsProductoListaTableAdapters.OPT_ProductoTableAdapter();

            ReportViewer reportViewer1 = new ReportViewer();
            ReportDataSource rds = new ReportDataSource();

            try
            {
                rds.Name = "dsProductoLista";
                rds.Value = ds.GetData();

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.LocalReport.ReportPath = "Areas/Productos/Rpt/rptProductoLista.rdlc";
                reportViewer1.LocalReport.Refresh();

                //reportViewer1.RefreshReport();
                byte[] bytes = reportViewer1.LocalReport.Render("Excel"
                                                               , null
                                                               , out mimeType
                                                               , out encoding
                                                               , out extension
                                                               , out streams
                                                               , out warnings);

                var NomArchivo = "Productos_" + DateTime.Now.ToString("ddMMyyyy") + "." + extension;

                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.BufferOutput = true;
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + NomArchivo);

                Response.BinaryWrite(bytes);                            // crea el archivo....
                Response.Flush();

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
            }
        }
    }

}