using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Data;
using System.IO;
using System.Configuration;
using Microsoft.Reporting.WebForms;
using PagedList;

using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using ExcelDataReader;

using OPT.Web.Areas.OrdenTrabajo.Models.Pago;
using OPT.Web.Models;
using OPT.Web.Libreria;

namespace OPT.Web.Areas.OrdenTrabajo.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class PagoController : Controller
    {
        SessionDTO mySession;

        public PagoController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }

        public ActionResult Index(long id)
        {
            OPT_OrdenDeTrabajoDAL ordenDeTrabajoDAL = new OPT_OrdenDeTrabajoDAL();
            OPT_FormaPagoDal FormaPagoDal = new OPT_FormaPagoDal();
            PagoResponse response = new PagoResponse();

            try
            {
                var OT = ordenDeTrabajoDAL.Buscar(id);
                var TotalPago = (OT.OPT_Pago.Sum(x => x.Monto));

                response.idOT = OT.idOT;
                response.RutCliente = OT.RutCliente;
                response.NombreCliente = OT.OPT_Cliente.Nombre;
                response.Precio = decimal.Parse(OT.Precio.ToString());
                response.Abono = decimal.Parse(OT.Abono.ToString()) + TotalPago;
                response.Saldo = response.Precio - response.Abono;
                response.Pagos = OT.OPT_Pago.ToList();
                response.EstadoPago = OT.EstadoPago.ToUpper();

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
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(PagoRequest item)
        {
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();
            OPT_PagoDAL DatoPago = new OPT_PagoDAL();
            OPT_Pago ItemPago = new OPT_Pago();

            try
            {
                #region ======================== INGRESO PAGO ========================

                ItemPago.idOT = item.idOT;
                ItemPago.FechaPago = item.FechaPago;
                ItemPago.Monto = item.Monto;
                ItemPago.TipoPago = item.TipoPago;

                DatoPago.Insertar(ItemPago);

                #endregion

                #region ======================== ACTUALIZO OT ========================

                var ItemOT = DatoOT.Buscar(item.idOT);
                var TotalPago = ItemOT.OPT_Pago.Sum(x => x.Monto) + decimal.Parse(ItemOT.Abono.ToString());
                var Saldo = ItemOT.Precio - TotalPago;

                if (Saldo <= 0)
                {
                    ItemOT.EstadoPago = "PAGADO";
                    DatoOT.Editar(ItemOT);
                }

                #endregion

                return Json(new { ok = true, respuesta = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                return Json(new { ok = false, respuesta = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult PagoMasivo()
        {
            PagoMasivoView response = new PagoMasivoView();
            OPT_EmpresaDAL EmpresaDal = new OPT_EmpresaDAL();

            response.Empresas = (from A in EmpresaDal.ComboEmpresa(mySession.idSucursal)
                                 select (new SelectListItem
                                 {
                                     Text = A.Empresa,
                                     Value = A.idEmpresa.ToString()
                                 })).ToList();

            return View(response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PagoMasivo(PagoMasivoRequest myForm)
        {
            JsonResponse<List<ErrorCargaMasivaDTO>> jsonResponse = new JsonResponse<List<ErrorCargaMasivaDTO>>();
            ErrorCargaMasivaDTO ItemError;

            OPT_Pago ItemPago = new OPT_Pago();
            OPT_PagoDAL PagoDal = new OPT_PagoDAL();
            OPT_OrdenDeTrabajoDAL OtDAL = new OPT_OrdenDeTrabajoDAL();
            OPT_OrdenDeTrabajo ordenDeTrabajo = new OPT_OrdenDeTrabajo();
            OPT_BitacoraOT bitacoraOT = new OPT_BitacoraOT();
            OPT_BitacoraOtDAL bitacoraOtDAL = new OPT_BitacoraOtDAL();

            int cont = 1;
            int contIngreso = 0;
            int idOT = 0;
            decimal pMonto = 0;
            bool FormatoArchivo = true;

            var ListOtPendiente = OtDAL.ListaPendiente(myForm.idEmpresa);

            try
            {
                HttpPostedFileBase file = Request.Files["Archivo"];

                if (file != null && file.ContentLength > 0)
                {
                    #region ======================== VALIDA ARCHIVO ========================
                    // ExcelDataReader works with the binary Excel file, so it needs a FileStream
                    // to get started. This is how we avoid dependencies on ACE or Interop:
                    Stream stream = file.InputStream;

                    // We return the interface, so that
                    IExcelDataReader reader = null;

                    if (file.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (file.FileName.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        throw new Exception("Formato de Archivo no soportado");
                    }

                    #endregion

                    #region ======================== CARGA DATOS ARCHIVOS ========================

                    while (reader.Read())
                    {
                        if (cont == 1)
                        {
                            #region =============== VALIDA CABECERA DEL EXCEL ===============

                            var Campo1 = reader.GetString(0).ToString().Trim().ToUpper();
                            var Campo2 = reader.GetString(1).ToString().Trim().ToUpper();

                            if (Campo1 != "OT" || Campo2 != "MONTO")
                            {
                                FormatoArchivo = false;
                                break;
                            }


                            #endregion
                        }
                        else if (cont > 1)
                        {
                            try
                            {
                                if (reader.IsDBNull(0))
                                {
                                    break;
                                }

                                idOT = int.Parse(reader.GetValue(0).ToString().Trim());
                                pMonto = decimal.Parse(reader.GetValue(1).ToString().Trim());

                                ordenDeTrabajo = ListOtPendiente.Where(x => x.idOT == idOT).FirstOrDefault();

                                if (ordenDeTrabajo == null)
                                {
                                    ItemError = new ErrorCargaMasivaDTO();
                                    ItemError.Fila = cont.ToString();
                                    ItemError.OT = idOT.ToString();
                                    ItemError.Error = "OT NO ENCONTRADA O NO TIENE SALDO PENDIENTE";
                                    jsonResponse.Respuesta.Add(ItemError);
                                }
                                else
                                {
                                    decimal SumPago = ordenDeTrabajo.OPT_Pago.Sum(x => x.Monto);
                                    decimal SaldoPendiente = decimal.Parse(ordenDeTrabajo.Saldo.ToString()) - SumPago;                                   

                                    if (SaldoPendiente >= pMonto)
                                    {
                                        ItemPago = new OPT_Pago();
                                        ItemPago.idOT = ordenDeTrabajo.idOT;
                                        ItemPago.Monto = pMonto;
                                        ItemPago.FechaPago = myForm.FechaPago;
                                        ItemPago.TipoPago = "TRANSFERENCIA";
                                        PagoDal.Insertar(ItemPago);

                                        SumPago += pMonto;

                                        if(SumPago>= ordenDeTrabajo.Saldo)
                                        {
                                            ordenDeTrabajo.EstadoPago = "PAGADO";
                                            OtDAL.Editar(ordenDeTrabajo);
                                        }

                                        contIngreso++;

                                       var Entregado = bitacoraOtDAL.Listar(int.Parse(ordenDeTrabajo.idOT.ToString()))
                                                                    .Any(x => x.idEstado == (int)EnumEstadoOt.Entregado);
                                        if(!Entregado)
                                        {
                                            bitacoraOT = new OPT_BitacoraOT();
                                            bitacoraOT.idOT = ordenDeTrabajo.idOT;
                                            bitacoraOT.idEstado = (int)EnumEstadoOt.Entregado;
                                            bitacoraOT.Responsable = mySession.Nombre;
                                            bitacoraOT.Fecha = DateTime.Now;
                                            bitacoraOT.Observacion = "Se realiza Pago Masivo";
                                            bitacoraOtDAL.Insertar(bitacoraOT);
                                        }
                                    }
                                    else
                                    {
                                        ItemError = new ErrorCargaMasivaDTO();
                                        ItemError.Fila = cont.ToString();
                                        ItemError.OT = ordenDeTrabajo.idOT.ToString();
                                        ItemError.OT = idOT.ToString();
                                        ItemError.Error = "Monto a Pagar (" + pMonto.ToString("N0") +") es mayor al monto adeudado " + SaldoPendiente.ToString("N0");
                                        jsonResponse.Respuesta.Add(ItemError);
                                    } 
                                }
                            }
                            catch (Exception ex)
                            {
                                Libreria.LogError.setLogExeption(ex);

                                ItemError = new ErrorCargaMasivaDTO();
                                ItemError.Fila = cont.ToString();
                                ItemError.OT = ordenDeTrabajo == null ? "0" : ordenDeTrabajo.idOT.ToString();
                                ItemError.OT = idOT.ToString();
                                ItemError.Nombre = reader.GetValue(3).ToString();
                                ItemError.Error = ex.Message;
                                jsonResponse.Respuesta.Add(ItemError);
                            }
                        }

                        cont++;
                    }

                    #endregion


                    if (FormatoArchivo == false)
                    {
                        throw new Exception("El formato del archivo no es el correcto.");
                    }
                }
                else
                {
                    throw new Exception("Archivo Vacio");
                }

                jsonResponse.Mensaje = "Se ingresaron " + contIngreso + " datos correctamente.";

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                jsonResponse.ok = false;
                jsonResponse.Mensaje = ex.Message;
            }

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);

        }
    }
}