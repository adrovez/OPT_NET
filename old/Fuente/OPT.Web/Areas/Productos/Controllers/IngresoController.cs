using Newtonsoft.Json;
using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using OPT.Web.Areas.Productos.Models;
using OPT.Web.Libreria;
using OPT.Web.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.Productos.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class IngresoController : Controller
    {
        SessionDTO mySession;

        public IngresoController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        #region ==================== LISTAR ====================
        // GET: Empresa/Empresa
        public ActionResult Index()
        {
            OPT_ProductoDocumentoDAL Datos = new OPT_ProductoDocumentoDAL();
            try
            {
                var Lista = Datos.Lista(1, "");
                return View(Lista);
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
            OPT_ProductoDocumentoDAL Datos = new OPT_ProductoDocumentoDAL();
            try
            {
                var pFiltro = myForm["Filtro"].ToString();
                var Lista = Datos.Lista(1, pFiltro);

                return PartialView("_ParcialListaIndex", Lista);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        #endregion

        // GET: Productos/Ingreso/Details/5
        public ActionResult Details(long id)
        {
            OPT_ProductoDocumentoDAL productoDocumentoDAL = new OPT_ProductoDocumentoDAL();
            OPT_ProductoDAL productoDAL = new OPT_ProductoDAL();
            try
            {
                var Documento = productoDocumentoDAL.Buscar(id);
                var ListProducto = Documento.OPT_ProductoIngresoEgreso.Select(x => x.idProducto).ToList();
                var Producto = productoDAL.Lista(ListProducto);
                DocumentoResponse response = new DocumentoResponse();


                response.NumDocumento = Documento.NumDocumento;
                response.TipoDocumento = Documento.OPT_TipoDocumento.TipoDocumento;
                response.FechaDocumento = Documento.FechaDocumento;
                response.Proveedor = Documento.Proveedor;

                response.Productos = (from A in Documento.OPT_ProductoIngresoEgreso
                                      select new DetalleDocumento
                                      {
                                          Cantidad = A.Cantidad,
                                          ValorUnitario = A.ValorUnitario,
                                          Producto = Producto.First(x=>x.idProducto == A.idProducto).Producto

                                      }).ToList();

                return View("_ModalDetails", response);
            }
            catch(Exception ex)
            {
                LogError.setLogExeption(ex);
                throw ex;
            }           
        }

        // GET: Productos/Ingreso/Create
        public ActionResult Create()
        {
            OPT_TipoDocumentoDAL tipoDocumentoDAL = new OPT_TipoDocumentoDAL();
            OPT_ProductoDAL ProductoDal = new OPT_ProductoDAL();
            IngresoResponse response = new IngresoResponse();
            try
            {
                response.ListTipoDocumento = (from A in tipoDocumentoDAL.Lista(1)
                                              select new SelectListItem
                                              {
                                                  Text = A.TipoDocumento,
                                                  Value = A.idTipoDocumento.ToString()
                                              }).ToList();


                response.ListProductos = (from A in ProductoDal.Lista("")
                                          select (new SelectListItem
                                          {
                                              Text = A.Producto,
                                              Value = A.idProducto.ToString()
                                          })).ToList();

                return View(response);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Documento(IngresoDocumentoRequest item)
        {
            OPT_ProductoDocumentoDAL productoDocumentoDAL = new OPT_ProductoDocumentoDAL();
            OPT_ProductoIngresoEgresoDAL ingresoEgresoDAL = new OPT_ProductoIngresoEgresoDAL();
            OPT_ProductoDAL productoDAL = new OPT_ProductoDAL();

            OPT_ProductoDocumento productoDocumento = new OPT_ProductoDocumento();
            JsonResponse<long> response = new JsonResponse<long>();
            try
            {
                //Rescato Lista de Productos
                List<Producto> ListProductos = JsonConvert.DeserializeObject<List<Producto>>(Request["ListProductos"]);

                #region =================== INGRESA DOCUMENTO ===================
                productoDocumento.idTipoDocumento = item.idTipoDocumento;
                productoDocumento.NumDocumento = item.NumDocumento;
                productoDocumento.Proveedor = item.Proveedor.ToUpper().Trim();
                productoDocumento.IngresoEgreso = 1;
                productoDocumento.FechaDocumento = item.FechaDocumento;
                productoDocumento.FechaIngreso = DateTime.Now;
                productoDocumentoDAL.Insertar(productoDocumento);

                #endregion

                #region =================== INGRESA PRODUCTOS ===================
                List<OPT_ProductoIngresoEgreso> Ingresos = new List<OPT_ProductoIngresoEgreso>();
                foreach (var x in ListProductos)
                {
                    Ingresos.Add(new OPT_ProductoIngresoEgreso
                    {
                        idProductoDocumento = productoDocumento.idProductoDocumento,
                        idProducto = x.idProducto,
                        Cantidad = x.Cantidad,
                        ValorUnitario = x.ValorUnitario
                    });
                }

                ingresoEgresoDAL.Insertar(Ingresos);

                #endregion

                var resp=productoDAL.ActualizaStock();

                if(resp.OK !=0)
                {
                    response.ok = false;
                    response.Mensaje = "Documento Ingresado Correctamente. Error al actualizar Stock.";

                    Exception ex = new Exception(resp.Mensaje);
                    LogError.setLogExeption(ex);
                }

                response.Respuesta = productoDocumento.idProductoDocumento;

            }
            catch (Exception ex)
            {
                LogError.setLogExeption(ex);
                response.ok = false;
                response.Mensaje = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }


    }
}
