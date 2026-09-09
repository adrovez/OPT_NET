using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using OPT.Web.Areas.Flujo.Models;
using OPT.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.Flujo.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class EnviarController : Controller
    {
        SessionDTO mySession;

        public EnviarController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }


        [HttpGet]
        public ActionResult EnviarFlujo(long id)
        {
            EnviarFlujoResponse response = new EnviarFlujoResponse();
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();
            OPT_EstadoOtDAL estadoOtDAL = new OPT_EstadoOtDAL();

            try
            {
                var Item = DatoOT.Buscar(id);

                response.idOt = Item.idOT;
                response.FechaAntencion = DateTime.Parse(Item.FechaAtencion.ToString());
                response.RutCliente = Item.OPT_Cliente.RutCliente;
                response.NombreCliente = Item.OPT_Cliente.RutCliente;
                response.Empresa = Item.OPT_Empresa.Empresa;
                response.idEstado = Item.OPT_BitacoraOT.OrderByDescending(x => x.Fecha).First().idEstado;
                response.Estado = estadoOtDAL.Buscar(response.idEstado).Estado;
                var EstadoSiguiente = response.idEstado + 1;

                response.ListEstados = (from A in estadoOtDAL.Listar().Where(x => x.idEstadoOT != 1)
                                        select (new SelectListItem
                                        {
                                            Text = A.Estado,
                                            Value = A.idEstadoOT.ToString(),
                                            Selected = EstadoSiguiente == A.idEstadoOT ? true : false

                                        })).ToList();


                return PartialView("_ModalEnviarFlujo", response);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EnviarFlujo(EnviarFlujoRequest Item)
        {
            JsonResponse<int> response = new JsonResponse<int>();
            OPT_BitacoraOtDAL oPT_BitacoraOtDAL = new OPT_BitacoraOtDAL();
            OPT_BitacoraOT bitacoraOT = new OPT_BitacoraOT();

            try
            {
                bitacoraOT.idOT = Item.idOT;
                bitacoraOT.idEstado = Item.idEstado;
                bitacoraOT.Fecha = DateTime.Now;
                bitacoraOT.Responsable = mySession.Nombre;
                bitacoraOT.Observacion = Item.Observacion == null ? "Sin Observación" : Item.Observacion;
                oPT_BitacoraOtDAL.Insertar(bitacoraOT);


            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                response.ok = false;
                response.Mensaje = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Bitacora(int id)
        {
            List<BitacoraResponse> response = new List<BitacoraResponse>();
            OPT_BitacoraOtDAL bitacoraOtDAL = new OPT_BitacoraOtDAL();
            OPT_EstadoOtDAL estadoOtDAL = new OPT_EstadoOtDAL();

            try
            {
                var Estados = estadoOtDAL.Listar();

                response = (from A in bitacoraOtDAL.Listar(id)
                            orderby A.Fecha descending
                            select new BitacoraResponse()
                            {
                                idBitacoraOT = A.idBitacoraOT,
                                idOT = A.idOT,
                                Estado = Estados.First(x => x.idEstadoOT == A.idEstado).Estado,
                                Fecha = A.Fecha,
                                Responsable = A.Responsable,
                                Observacion = A.Observacion
                            }).ToList();

                return PartialView("_ModalBitacora", response);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }
    }
}
