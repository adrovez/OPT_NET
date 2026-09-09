using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using OPT.Web.Areas.OrdenTrabajo.Models;
using OPT.Web.Areas.OrdenTrabajo.Models.OrdenTrabajo;
using OPT.Web.Libreria;
using OPT.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace OPT.Web.Areas.OrdenTrabajo.Controllers
{
    [Authorize]
    [Libreria.Autenticado]
    public class IngresoController : Controller
    {
        SessionDTO mySession;

        public IngresoController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        #region ============ LISTADO ============

        public ActionResult Index()
        {
            OPT_OrdenDeTrabajoDAL Datos = new OPT_OrdenDeTrabajoDAL();
            OPT_EmpresaDAL DatoEmpresa = new OPT_EmpresaDAL();
            OPT_EstadoOtDAL estadoOtDAL = new OPT_EstadoOtDAL();
            OrdenTrabajoViewResponse response = new OrdenTrabajoViewResponse();

            try
            {
                response.Empresas = (from A in DatoEmpresa.ComboEmpresa(mySession.idSucursal)
                                     select new SelectListItem
                                     {
                                         Text = A.Empresa,
                                         Value = A.idEmpresa.ToString()
                                     }).ToList();

                response.Estados = estadoOtDAL.Listar();

                response.OrdenDeTrabajos = Datos.Lista(0, 0, "", mySession.idSucursal);

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
        public PartialViewResult Paginar(FormCollection myForm)
        {
            OPT_OrdenDeTrabajoDAL Datos = new OPT_OrdenDeTrabajoDAL();
            OrdenTrabajoViewResponse response = new OrdenTrabajoViewResponse();
            OPT_EstadoOtDAL estadoOtDAL = new OPT_EstadoOtDAL();

            try
            {
                var pOT = myForm["NumeroOT"].ToString() == "" ? 0 : int.Parse(myForm["NumeroOT"].ToString()); ;
                var pRut = myForm["RutNombre"].ToString();
                var pIdEmpresa = myForm["idEmpresa"] == "" ? 0 : int.Parse(myForm["idEmpresa"].ToString());

                response.Estados = estadoOtDAL.Listar();

                response.OrdenDeTrabajos = Datos.Lista(pOT, pIdEmpresa, pRut, mySession.idSucursal).ToList();

                return PartialView("_ParcialIndex", response);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        #endregion

        #region ============ DETALLE ============
        [HttpGet]
        public PartialViewResult Details(long id)
        {
            OrdenTrabajoDetalleResponse response = new OrdenTrabajoDetalleResponse();
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();
            OPT_OrdenDeTrabajoDetalleDAL ordenDeTrabajoDetalleDAL = new OPT_OrdenDeTrabajoDetalleDAL();
            OPT_AbonoDAL abonoDAL = new OPT_AbonoDAL();

            try
            {
                var Item = DatoOT.Buscar(id);
                var ItemAbono = abonoDAL.Listar(id);
                var ItemProductos = ordenDeTrabajoDetalleDAL.Lista(id);

                response.idOT = Item.idOT;
                response.Beneficiario = Item.Beneficiario;
                response.RutCliente = Item.OPT_Cliente.RutCliente;
                //response.NombreCliente = Item.OPT_Cliente.Nombre;
                //response.FechaNacimiento = Item.OPT_Cliente.FechaNacimiento == null ? "" : DateTime.Parse(Item.OPT_Cliente.FechaNacimiento.ToString()).ToShortDateString();
                //response.Comuna = Item.OPT_Cliente.OPT_Comuna.Comuna;
                //response.Region = Item.OPT_Cliente.OPT_Comuna.OPT_Region.Region;
                //response.Direccion = Item.OPT_Cliente.Direccion;
                response.Cliente = Item.OPT_Cliente;
                response.FechaAtencion = DateTime.Parse(Item.FechaAtencion.ToString());
                response.FechaEntrega = DateTime.Parse(Item.FechaEntrega.ToString());
                response.HoraEntrega = TimeSpan.Parse(Item.HoraEntrega.ToString());
                response.Precio = Item.Precio;
                response.Abono = Item.Abono;
                response.Saldo = Item.Saldo;
                response.NumeroCuota = Item.NumeroCuota;
                response.EstadoPago = Item.EstadoPago;
                //response.EtapaOT = Item.OPT_BitacoraOT.First().OPT_EstadoOT.Estado;
                response.NombreEmpresa = Item.OPT_Empresa.Empresa;

                response.Abonos = (from A in ItemAbono
                                   select new AbonoResponse
                                   {
                                       FormaPago = A.OPT_FormaPago.FormaPago,
                                       Monto = A.Monto,
                                       FechaAbono = A.FechaAbono,
                                   }).ToList();

                response.Productos = (from A in ItemProductos
                                      select new ProductoResponse
                                      {
                                          Producto = A.OPT_Producto.Producto,
                                          Cantidad = A.Cantidad,
                                          Valor = A.ValorUnitario,
                                          Comentario = A.Comentario
                                      }).ToList();

                response.Cristales = Item.OPT_RecetaCristales.FirstOrDefault();

                return PartialView("_ModalDetalle", response);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }
        #endregion

        #region ============ NUEVO ============


        public ActionResult Create(string id = "", int idAtencion = 0)
        {
            OPT_RegionDAL RegionDal = new OPT_RegionDAL();
            OPT_ComunaDAL comunaDAL = new OPT_ComunaDAL();
            OPT_EmpresaDAL EmpresaDal = new OPT_EmpresaDAL();
            OPT_ProductoDAL ProductoDal = new OPT_ProductoDAL();
            OrdenTrabajoResponse response = new OrdenTrabajoResponse();
            OPT_FormaPagoDal FormaPagoDal = new OPT_FormaPagoDal();
            OPT_ClienteDAL clienteDAL = new OPT_ClienteDAL();

            try
            {
                if (id != "")
                {
                    var cliente = clienteDAL.Buscar(id);

                    response.idAtencion = idAtencion;
                    response.cliente.RutCliente = cliente.RutCliente;
                    response.cliente.idEmpresa = cliente.idEmpresa;
                    response.cliente.Empresa = cliente.OPT_Empresa.Empresa;
                    response.cliente.Nombre = cliente.Nombre;
                    response.cliente.Direccion = cliente.Direccion;
                    response.cliente.idComuna = cliente.idComuna;
                    response.cliente.Comuna = cliente.OPT_Comuna.Comuna;
                    response.cliente.idRegion = cliente.OPT_Comuna.idRegion;
                    response.cliente.Region = cliente.OPT_Comuna.OPT_Region.Region;
                    response.cliente.Celular = cliente.Celular;
                    response.cliente.Mail = cliente.Mail;
                    response.cliente.FechaIngreso = cliente.FechaIngreso;
                    response.cliente.FechaNacimiento = cliente.FechaNacimiento;


                    response.cliente.ListComuna = (from A in comunaDAL.Lista(int.Parse(response.cliente.idRegion.ToString()))
                                                   select (new SelectListItem
                                                   {
                                                       Text = A.Comuna,
                                                       Value = A.idComuna.ToString(),
                                                       Selected = response.cliente == null ? false : response.cliente.idComuna == A.idComuna ? true : false
                                                   })).ToList();
                }

                response.cliente.ListRegion = (from A in RegionDal.Lista()
                                               select (new SelectListItem
                                               {
                                                   Text = A.Region,
                                                   Value = A.idRegion.ToString(),
                                                   Selected = response.cliente == null ? false : response.cliente.idRegion == A.idRegion ? true : false

                                               })).ToList();



                response.cliente.ListEmpresa = (from A in EmpresaDal.ComboEmpresa(mySession.idSucursal)
                                                select (new SelectListItem
                                                {
                                                    Text = A.Empresa,
                                                    Value = A.idEmpresa.ToString(),
                                                    Selected = response.cliente == null ? false : response.cliente.idEmpresa == A.idEmpresa ? true : false
                                                })).ToList();

                response.producto.ListProducto = (from A in ProductoDal.Lista("")
                                                  select (new SelectListItem
                                                  {
                                                      Text = A.Producto,
                                                      Value = A.idProducto.ToString()
                                                  })).ToList();


                response.ListFormaPago = (from A in FormaPagoDal.Lista()
                                          select (new SelectListItem
                                          {
                                              Text = A.FormaPago,
                                              Value = A.idFormaPago.ToString()
                                          })).ToList();

                return View(response);
            }
            catch (Exception ex)
            {
                LogError.setLogExeption(ex);
                throw ex;
            }
        }
        #endregion

        #region ============ EDITAR ============


        public ActionResult Edit(long id)
        {
            OrdenTrabajoResponse response = new OrdenTrabajoResponse();

            OPT_AtencionDAL atencionDAL = new OPT_AtencionDAL();
            OPT_OrdenDeTrabajoDAL ordenDeTrabajoDAL = new OPT_OrdenDeTrabajoDAL();
            OPT_OrdenDeTrabajoDetalleDAL ordenDeTrabajoDetalleDAL = new OPT_OrdenDeTrabajoDetalleDAL();
            OPT_RegionDAL RegionDal = new OPT_RegionDAL();
            OPT_ComunaDAL comunaDAL = new OPT_ComunaDAL();
            OPT_EmpresaDAL EmpresaDal = new OPT_EmpresaDAL();
            OPT_ProductoDAL ProductoDal = new OPT_ProductoDAL();
            OPT_FormaPagoDal FormaPagoDal = new OPT_FormaPagoDal();


            try
            {
                var Ot = ordenDeTrabajoDAL.Buscar(id);
                var atencion = atencionDAL.BuscarByIdOt(id);
                var receta = Ot.OPT_RecetaCristales.FirstOrDefault(x => x.idOT == id);
                var formapago = FormaPagoDal.Lista();

                #region ================= OT =================
                response.idOT = Ot.idOT;
                response.idSucursal = Ot.idSucursal;
                response.idEmpresa = Ot.idEmpresa;
                response.Beneficiario = Ot.Beneficiario;
                response.RutCliente = Ot.RutCliente;
                response.FechaAtencion = Ot.FechaAtencion;
                response.FechaEntrega = Ot.FechaEntrega;
                response.HoraEntrega = Ot.HoraEntrega;
                response.Precio = Ot.Precio;
                response.Abono = Ot.Abono;
                response.Saldo = Ot.Saldo;
                response.NumeroCuota = Ot.NumeroCuota;
                response.Usuario = Ot.Usuario;
                response.idAtencion = atencion == null ? 0 : atencion.idAtencion;
                response.idAnamnesis = atencion == null ? 0 : atencion.idAnamnesis == null ? 0 : long.Parse(atencion.idAnamnesis.ToString());

                #endregion

                #region ================= CLIENTE =================

                response.cliente.RutCliente = Ot.OPT_Cliente.RutCliente;
                response.cliente.idEmpresa = Ot.OPT_Cliente.idEmpresa;
                response.cliente.Empresa = Ot.OPT_Cliente.OPT_Empresa.Empresa;
                response.cliente.Nombre = Ot.OPT_Cliente.Nombre;
                response.cliente.Direccion = Ot.OPT_Cliente.Direccion;
                response.cliente.idComuna = Ot.OPT_Cliente.idComuna;
                response.cliente.Comuna = Ot.OPT_Cliente.OPT_Comuna.Comuna;
                response.cliente.idRegion = Ot.OPT_Cliente.OPT_Comuna.idRegion;
                response.cliente.Region = Ot.OPT_Cliente.OPT_Comuna.OPT_Region.Region;
                response.cliente.Celular = Ot.OPT_Cliente.Celular;
                response.cliente.Mail = Ot.OPT_Cliente.Mail;
                response.cliente.FechaIngreso = Ot.OPT_Cliente.FechaIngreso;
                response.cliente.FechaNacimiento = Ot.OPT_Cliente.FechaNacimiento;

                #endregion

                #region ================= RECETA =================

                if (receta != null)
                {
                    response.receta.idRecetaCristales = receta.idRecetaCristales;
                    response.receta.idOT = receta.idRecetaCristales;
                    response.receta.LejosODEsferico = receta.LejosODEsferico;
                    response.receta.LejosODCilindro = receta.LejosODCilindro;
                    response.receta.LejosODEje = receta.LejosODEje;
                    response.receta.LejosODObservacion = receta.LejosODObservacion;
                    response.receta.LejosOIEsferico = receta.LejosOIEsferico;
                    response.receta.LejosOICilindro = receta.LejosOICilindro;
                    response.receta.LejosOIEje = receta.LejosOIEje;
                    response.receta.LejosOIObservacion = receta.LejosOIObservacion;
                    response.receta.LejosDPEsferico = receta.LejosDPEsferico;
                    response.receta.LejosDPObservacion = receta.LejosDPObservacion;
                    response.receta.CercaODEsferico = receta.CercaODEsferico;
                    response.receta.CercaODCilindro = receta.CercaODCilindro;
                    response.receta.CercaODEje = receta.CercaODEje;
                    response.receta.CercaODObservacion = receta.CercaODObservacion;
                    response.receta.CercaOIEsferico = receta.CercaOIEsferico;
                    response.receta.CercaOICilindro = receta.CercaOICilindro;
                    response.receta.CercaOIEje = receta.CercaOIEje;
                    response.receta.CercaOIObservacion = receta.CercaOIObservacion;
                    response.receta.LejosADDEsfera = receta.LejosADDEsfera;
                    response.receta.CercaDPEsferico = receta.CercaDPEsferico;
                    response.receta.CercaDPObservacion = receta.CercaDPObservacion;
                    response.receta.CheckLejos = receta.CheckLejos;
                    response.receta.CheckCerca = receta.CheckCerca;
                    response.receta.CheckCristalesLaboratorio = receta.CheckCristalesLaboratorio;
                    response.receta.CheckUrgente = receta.CheckUrgente;
                }


                #endregion

                #region ================= DETALLE OT =================

                response.detalleOT = ordenDeTrabajoDetalleDAL.Lista(Ot.idOT);

                #endregion

                #region =============== ABONOS ======================
                foreach (var abono in Ot.OPT_Abono.ToList())
                {
                    response.abonos.Add(new AbonoResponse
                    {
                        idAbono = abono.idAbono,
                        idOT = abono.idOT,
                        idFormaPago = abono.idFormaPago,
                        FormaPago = formapago.FirstOrDefault(x => x.idFormaPago == abono.idFormaPago).FormaPago,
                        Monto = abono.Monto,
                        FechaAbono = abono.FechaAbono
                    });
                }
                #endregion


                response.cliente.ListComuna = (from A in comunaDAL.Lista(int.Parse(response.cliente.idRegion.ToString()))
                                               select (new SelectListItem
                                               {
                                                   Text = A.Comuna,
                                                   Value = A.idComuna.ToString(),
                                                   Selected = response.cliente == null ? false : response.cliente.idComuna == A.idComuna ? true : false
                                               })).ToList();


                response.cliente.ListRegion = (from A in RegionDal.Lista()
                                               select (new SelectListItem
                                               {
                                                   Text = A.Region,
                                                   Value = A.idRegion.ToString(),
                                                   Selected = response.cliente == null ? false : response.cliente.idRegion == A.idRegion ? true : false

                                               })).ToList();



                response.cliente.ListEmpresa = (from A in EmpresaDal.ComboEmpresa(mySession.idSucursal)
                                                select (new SelectListItem
                                                {
                                                    Text = A.Empresa,
                                                    Value = A.idEmpresa.ToString(),
                                                    Selected = response.cliente == null ? false : response.cliente.idEmpresa == A.idEmpresa ? true : false
                                                })).ToList();

                response.producto.ListProducto = (from A in ProductoDal.Lista("")
                                                  select (new SelectListItem
                                                  {
                                                      Text = A.Producto,
                                                      Value = A.idProducto.ToString()
                                                  })).ToList();


                response.ListFormaPago = (from A in formapago
                                          select (new SelectListItem
                                          {
                                              Text = A.FormaPago,
                                              Value = A.idFormaPago.ToString()
                                          })).ToList();

                return View("Create", response);
            }
            catch (Exception ex)
            {
                LogError.setLogExeption(ex);
                throw ex;
            }
        }
        #endregion

        #region ============ ELIMINAR ============
        [HttpGet]
        public JsonResult Delete(int id)
        {
            OPT_OrdenDeTrabajoDAL ordenDeTrabajoDAL = new OPT_OrdenDeTrabajoDAL();

            JsonResponse<int> response = new JsonResponse<int>();
            try
            {
                var SpResponse = ordenDeTrabajoDAL.Eliminar(id);

                if (SpResponse.OK != 0)
                {
                    response.ok = false;
                    response.Mensaje = SpResponse.Mensaje;
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

        #region ============ ORDEN DE TRABAJO ============
        [HttpPost]
        public JsonResult Existe(long id)
        {
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();

            try
            {
                var existe = DatoOT.Existe(id);

                if (existe)
                {
                    throw new Exception("OT N° " + id + " fue ingresado anteriormente.");
                }

                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion

        #region ============ CLIENTE ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cliente(ClienteRequest item)
        {
            OPT_Cliente ItemCliente = new OPT_Cliente();
            OPT_OrdenDeTrabajo ItemOT = new OPT_OrdenDeTrabajo();

            OPT_ClienteDAL DatoCliente = new OPT_ClienteDAL();
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();

            OPT_BitacoraOtDAL oPT_BitacoraOtDAL = new OPT_BitacoraOtDAL();
            OPT_BitacoraOT bitacoraOT = new OPT_BitacoraOT();



            var existe = true;
            try
            {
                #region ======================= CLIENTE =======================

                ItemCliente = ClienteInsertar(item);

                #endregion

                #region ======================= ORDEN TRABAJO =======================

                ItemOT.idOT = item.idOT;
                ItemOT.idSucursal = mySession.idSucursal;
                ItemOT.idEmpresa = ItemCliente.idEmpresa;
                ItemOT.Usuario = mySession.Nombre;

                //ItemOT.Beneficiario = item.Beneficiario.ToUpper().Trim();
                ItemOT.RutCliente = item.RutCliente.ToUpper().Replace(".", "");
                ItemOT.Beneficiario = item.Beneficiario == null ? "" : item.Beneficiario.ToUpper().Trim();
                ItemOT.FechaAtencion = DateTime.Parse(item.FechaAtencion.ToString());
                ItemOT.FechaEntrega = DateTime.Parse(item.FechaEntrega.ToString());
                ItemOT.HoraEntrega = TimeSpan.Parse("12:00"); //item.HoraEntrega;

                var existeOT = DatoOT.Existe(ItemOT.idOT);
                if (existeOT)
                {
                    DatoOT.Editar(ItemOT);
                }
                else
                {
                    DatoOT.Insertar(ItemOT);
                }

                #endregion

                #region ======================= BITACORA =======================

                bitacoraOT.idOT = item.idOT;
                bitacoraOT.idEstado = (int)EnumEstadoOt.Ingresado;
                bitacoraOT.Fecha = DateTime.Now;
                bitacoraOT.Responsable = mySession.Nombre;
                bitacoraOT.Observacion = "Sin Observación";
                oPT_BitacoraOtDAL.Insertar(bitacoraOT);

                #endregion

                #region ======================= ATENCION =======================
                if (item.idAtencion > 0)
                {
                    OPT_AtencionDAL atencionDAL = new OPT_AtencionDAL();

                    var atencion = atencionDAL.Buscar(item.idAtencion);
                    atencion.idOT = item.idOT;

                    atencionDAL.Editar(atencion);
                }
                #endregion



                return Json(new { ok = true, respuesta = ItemOT.RutCliente }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public OPT_Cliente ClienteInsertar(ClienteRequest item)
        {
            OPT_Cliente ItemCliente = new OPT_Cliente();
            OPT_ClienteDAL DatoCliente = new OPT_ClienteDAL();
            var existe = true;
            try
            {
                #region ======================= CLIENTE =======================

                ItemCliente = DatoCliente.BuscarEditar(item.RutCliente.ToUpper().Replace(".", ""));

                if (ItemCliente == null)
                {
                    existe = false;
                    ItemCliente = new OPT_Cliente();
                    ItemCliente.RutCliente = item.RutCliente.ToUpper().Replace(".", "");
                    ItemCliente.FechaIngreso = DateTime.Now;
                }

                ItemCliente.idEmpresa = item.idEmpresa == null ? 1 : int.Parse(item.idEmpresa.ToString());
                ItemCliente.Nombre = item.Nombre.ToUpper();
                ItemCliente.idComuna = item.idComuna == null ? 0 : int.Parse(item.idComuna.ToString());
                ItemCliente.Direccion = item.Direccion == null ? "" : item.Direccion.ToUpper();
                ItemCliente.Celular = item.Celular == null ? "" : item.Celular.ToUpper();
                ItemCliente.Mail = item.Mail == null? "":  item.Mail.ToUpper();
                ItemCliente.FechaNacimiento = item.FechaNacimiento;
                ItemCliente.TipoPrevision = item.TipoPrevision == null ? "FONASA" : item.TipoPrevision;

                if (existe)
                {
                    DatoCliente.Editar(ItemCliente);
                }
                else
                {
                    DatoCliente.Insertar(ItemCliente);
                }
                #endregion

                return ItemCliente;
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult DatoCliente(string id)
        {
            OPT_RegionDAL RegionDal = new OPT_RegionDAL();
            OPT_ComunaDAL ComunaDal = new OPT_ComunaDAL();
            OPT_EmpresaDAL EmpresaDal = new OPT_EmpresaDAL();
            OPT_ClienteDAL ClienteDal = new OPT_ClienteDAL();
            ClienteResponse response = new ClienteResponse();

            try
            {
                var pRut = id.Replace(".", "").ToUpper();
                var ItemCliente = ClienteDal.Buscar(pRut);
                response.RutCliente = pRut;

                if (ItemCliente != null)
                {
                    response.Nombre = ItemCliente.Nombre;
                    response.Direccion = ItemCliente.Direccion;
                    response.Celular = ItemCliente.Celular;
                    response.Mail = ItemCliente.Mail;
                    response.FechaIngreso = ItemCliente.FechaIngreso;
                    response.idComuna = ItemCliente.idComuna;
                    response.idRegion = ItemCliente.OPT_Comuna.idRegion;
                    response.idEmpresa = ItemCliente.idEmpresa;
                    response.FechaNacimiento = ItemCliente.FechaNacimiento;
                    response.TipoPrevision = ItemCliente.TipoPrevision;

                    response.ListComuna = (from A in ComunaDal.Lista(ItemCliente.OPT_Comuna.idRegion)
                                           select (new SelectListItem
                                           {
                                               Text = A.Comuna,
                                               Value = A.idComuna.ToString(),
                                               Selected = ItemCliente.idComuna == A.idComuna ? true : false
                                           })).ToList();
                }

                return Json(new { ok = true, respuesta = response }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region ============ RECETA ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Receta(RecetaRequest item)
        {
            OPT_RecetaCristalesDAL recetaCristalesDAL = new OPT_RecetaCristalesDAL();
            OPT_RecetaCristales recetaCristales = new OPT_RecetaCristales();

            try
            {
                recetaCristales = RecetaInsertar(item);

                return Json(new { ok = true, respuesta = recetaCristales.idRecetaCristales }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public OPT_RecetaCristales RecetaInsertar(RecetaRequest item)
        {
            OPT_RecetaCristalesDAL recetaCristalesDAL = new OPT_RecetaCristalesDAL();
            OPT_RecetaCristales recetaCristales = new OPT_RecetaCristales();

            try
            {

                recetaCristales.idRecetaCristales = item.idRecetaCristales;

                if (item.idOT > 0)
                {
                    recetaCristales.idOT = item.idOT;
                }

                if (item.RutCliente.Length > 0)
                {
                    recetaCristales.RutCliente = item.RutCliente;
                }

                recetaCristales.FechaIngreso = item.FechaIngreso;
                recetaCristales.CheckCristalesLaboratorio = item.CheckCristalesLaboratorio == 1 ? true : false;
                recetaCristales.CheckUrgente = item.CheckUrgente == 1 ? true : false;
                recetaCristales.CheckCerca = item.CheckCerca == 1 ? true : false;
                recetaCristales.CheckLejos = item.CheckLejos == 1 ? true : false;

                recetaCristales.CercaODEsferico = item.CercaODEsferico;
                recetaCristales.CercaODCilindro = item.CercaODCilindro;
                recetaCristales.CercaODEje = item.CercaODEje;
                recetaCristales.CercaODObservacion = item.CercaODObservacion;
                recetaCristales.CercaOIEsferico = item.CercaOIEsferico;
                recetaCristales.CercaOICilindro = item.CercaOICilindro;
                recetaCristales.CercaOIEje = item.CercaOIEje;
                recetaCristales.CercaOIObservacion = item.CercaOIObservacion;
                recetaCristales.CercaDPEsferico = item.CercaDPEsferico;
                recetaCristales.CercaDPObservacion = item.CercaDPObservacion;

                recetaCristales.LejosODEsferico = item.LejosODEsferico;
                recetaCristales.LejosODCilindro = item.LejosODCilindro;
                recetaCristales.LejosODEje = item.LejosODEje;
                recetaCristales.LejosODObservacion = item.LejosODObservacion;
                recetaCristales.LejosOIEsferico = item.LejosOIEsferico;
                recetaCristales.LejosOICilindro = item.LejosOICilindro;
                recetaCristales.LejosOIEje = item.LejosOIEje;
                recetaCristales.LejosOIObservacion = item.LejosOIObservacion;
                recetaCristales.LejosDPEsferico = item.LejosDPEsferico;
                recetaCristales.LejosDPObservacion = item.LejosDPObservacion;
                recetaCristales.LejosADDEsfera = item.LejosADDEsfera;


                if (item.idRecetaCristales > 0)
                {
                    recetaCristalesDAL.Editar(recetaCristales);
                }
                else
                {
                    recetaCristalesDAL.Insertar(recetaCristales);
                }

                return recetaCristales;
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult BuscaReceta(BuscaRecetaRequest item)
        {
            JsonResponse<OPT_RecetaCristales> response = new JsonResponse<OPT_RecetaCristales>();
            OPT_RecetaCristalesDAL recetaCristalesDAL = new OPT_RecetaCristalesDAL();

            try
            {
                if (item.id > 0)
                {
                    response.Respuesta = recetaCristalesDAL.BuscarById(item.id);
                }
                else
                {
                    response.Respuesta = recetaCristalesDAL.Buscar(item.Rut);
                }


                if (response.Respuesta == null)
                {
                    response.Respuesta = new OPT_RecetaCristales();
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

        #region ============ LENTE ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Lente(ProductoRequest item)
        {
            OPT_OrdenDeTrabajoDetalle ordenDeTrabajoDetalle = new OPT_OrdenDeTrabajoDetalle();
            OPT_OrdenDeTrabajoDetalleDAL ordenDeTrabajoDetalleDAL = new OPT_OrdenDeTrabajoDetalleDAL();

            try
            {

                ordenDeTrabajoDetalle.idOT = item.idOT;
                ordenDeTrabajoDetalle.idProducto = item.idProducto;
                ordenDeTrabajoDetalle.Cantidad = item.Cantidad;
                ordenDeTrabajoDetalle.ValorUnitario = int.Parse(item.Valor.Replace(".", ""));
                ordenDeTrabajoDetalle.Comentario = item.Comentario == null ? "" : item.Comentario.Trim();
                ordenDeTrabajoDetalleDAL.Insertar(ordenDeTrabajoDetalle);

                return Json(new { ok = true, respuesta = ordenDeTrabajoDetalle.idOTDetalle }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult EliminaLente(long id)
        {
            OPT_OrdenDeTrabajoDetalleDAL ordenDeTrabajoDetalleDAL = new OPT_OrdenDeTrabajoDetalleDAL();

            try
            {
                ordenDeTrabajoDetalleDAL.Eliminar(id);

                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region ============ ABONO ============

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Abono(AbonoRequest item)
        {
            OPT_Abono abono = new OPT_Abono();
            OPT_AbonoDAL abonoDAL = new OPT_AbonoDAL();

            try
            {
                abono.idOT = item.idOT;
                abono.Monto = item.Monto;
                abono.idFormaPago = item.idFormaPago;
                abono.FechaAbono = DateTime.Now;
                abonoDAL.Insertar(abono);

                return Json(new { ok = true, respuesta = abono.idAbono }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult EliminaAbono(long id)
        {
            OPT_AbonoDAL abonoDAL = new OPT_AbonoDAL();

            try
            {
                var abono = abonoDAL.Buscar(id);
                abonoDAL.Eliminar(abono);

                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Finaliza(OrdenTrabajoRequest item)
        {
            OPT_OrdenDeTrabajoDAL oPT_OrdenDeTrabajoDAL = new OPT_OrdenDeTrabajoDAL();
            List<OPT_Cuota> ListaCuota = new List<OPT_Cuota>();
            OPT_Cuota ItemCuota = new OPT_Cuota();
            OPT_CuotaDAL DatoCuota = new OPT_CuotaDAL();
            OPT_BitacoraOtDAL oPT_BitacoraOtDAL = new OPT_BitacoraOtDAL();
            OPT_BitacoraOT bitacoraOT = new OPT_BitacoraOT();


            try
            {
                if (item.idEmpresa == 0)
                {
                    return Json(new { ok = false, respuesta = "Debe seleccionar una empresa para la OT N° " + item.idOT.ToString() }, JsonRequestBehavior.AllowGet);
                }

                if (item.Precio == 0 || item.Precio == null)
                {
                    return Json(new { ok = false, respuesta = "OT N° " + item.idOT.ToString() + " no tiene Monto, debe ingresar producto." }, JsonRequestBehavior.AllowGet);
                }

                var Ot = oPT_OrdenDeTrabajoDAL.Buscar(item.idOT);
                Ot.idEmpresa = item.idEmpresa;
                Ot.Precio = item.Precio;
                Ot.Abono = item.Abono == null ? 0 : item.Abono;
                Ot.Saldo = (item.Precio - Ot.Abono);
                Ot.NumeroCuota = item.NumeroCuota == null ? 0 : item.NumeroCuota;
                Ot.EstadoPago = Ot.Saldo > 0 ? "PENDIENTE" : "PAGADO";


                if (Ot.Saldo > 0 && Ot.NumeroCuota == 0)
                {
                    throw new Exception("Debe ingresar numero de cuotas para el saldo pendiente.");
                }

                oPT_OrdenDeTrabajoDAL.Editar(Ot);

                #region ======================= CUOTA =======================

                if (Ot.Saldo > 0 && Ot.NumeroCuota > 0)
                {
                    DatoCuota.Eliminar(Ot.idOT);
                    DateTime FechaInicioPago;

                    FechaInicioPago = DateTime.Parse(item.InicioPago.ToString());

                    for (int x = 1; x <= Ot.NumeroCuota; x++)
                    {

                        ItemCuota = new OPT_Cuota();
                        ItemCuota.idOT = Ot.idOT;
                        ItemCuota.Numero = x;
                        ItemCuota.FechaBencimiento = FechaInicioPago.AddMonths(x - 1);
                        ItemCuota.Estado = "PENDIENTE";
                        ItemCuota.ValorCuota = Math.Round(Convert.ToDecimal(Ot.Saldo / Ot.NumeroCuota));
                        ListaCuota.Add(ItemCuota);
                    }

                    if (ListaCuota.Count > 0)
                    {
                        DatoCuota.Insertar(ListaCuota);
                    }
                }
                #endregion

                #region ======================= BITACORA =======================

                bitacoraOT.idOT = Ot.idOT;
                bitacoraOT.idEstado = (int)EnumEstadoOt.EnProceso;
                bitacoraOT.Fecha = DateTime.Now;
                bitacoraOT.Responsable = mySession.Nombre;
                bitacoraOT.Observacion = "Sin Observación";
                oPT_BitacoraOtDAL.Insertar(bitacoraOT);

                #endregion

                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Finaliza(long id)
        {
            OPT_OrdenDeTrabajoDAL oPT_OrdenDeTrabajoDAL = new OPT_OrdenDeTrabajoDAL();

            FinalizaResponse response = new FinalizaResponse();

            try
            {
                var Ot = oPT_OrdenDeTrabajoDAL.Buscar(id);
                response.idOT = Ot.idOT;
                response.Cliente = Ot.OPT_Cliente.Nombre;
                response.Rut = Ot.RutCliente;
                response.Empresa = Ot.OPT_Empresa.Empresa;




                return PartialView(response);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;//return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}