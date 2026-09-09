using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Entidad.DTO;
using OPT.Web.Areas.Anamnesis.Models;
using OPT.Web.Areas.Anamnesis.Models.Ingresar;
using OPT.Web.Areas.OrdenTrabajo.Models;
using OPT.Web.Libreria;
using OPT.Web.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace OPT.Web.Areas.Anamnesis.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class AnamnesiController : Controller
    {
        SessionDTO mySession;

        public AnamnesiController()
        {
            mySession = new SessionDTO();
            Libreria.UsuarioConectado.ValidaSession(ref mySession);
        }



        #region ============ NUEVO ============
        public PartialViewResult Create(string id)
        {
            OPT_ClienteDAL clienteDAL = new OPT_ClienteDAL();
            AnamnesisCreateResponse response = new AnamnesisCreateResponse();

            try
            {
                response.Cliente = clienteDAL.Buscar(id);

                return PartialView("_ModalCreate", response);
            }
            catch (Exception ex)
            {
                LogError.setLogExeption(ex);
                throw ex;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Preguntas(AnamnesisPreguntasRequest item)
        {
            JsonResponse<long> response = new JsonResponse<long>();
            try
            {
                response.Respuesta = AnamnesisInsertar(item);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                response.ok = false;
                response.Mensaje = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public long AnamnesisInsertar(AnamnesisPreguntasRequest item)
        {
            OPT_Anamnesis anamnesis = new OPT_Anamnesis();
            OPT_AnamnesisDAL anamnesisDAL = new OPT_AnamnesisDAL();

            try
            {
                anamnesis.idEmpresa = int.Parse(item.idEmpresa.ToString());
                anamnesis.RutCliente = item.RutCliente.ToUpper().Replace(".", "");
                anamnesis.idSucursal = mySession.idSucursal;
                anamnesis.RutUsuario = mySession.Rut;
                anamnesis.hipertension = item.hipertension == 1 ? true : false;
                anamnesis.Diabetes = item.Diabetes == 1 ? true : false;
                anamnesis.Alergias = item.Alergias == 1 ? true : false;
                anamnesis.Lentes = item.Lentes == 1 ? true : false;
                anamnesis.Observacion = item.Observacion == null ? "" : item.Observacion.Trim();
                anamnesis.FechaIngreso = DateTime.Now;
                anamnesisDAL.Insertar(anamnesis);

                return anamnesis.idAnamnesis;
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
            AnamnesisDetalleResponse response = new AnamnesisDetalleResponse();
            OPT_AnamnesisDAL anamnesisDAL = new OPT_AnamnesisDAL();

            try
            {
                var Item = anamnesisDAL.Buscar(id);

                response.idAnamnesis = Item.idAnamnesis;
                response.RutCliente = Item.OPT_Cliente.RutCliente;
                response.NombreCliente = Item.OPT_Cliente.Nombre;
                response.NombreEmpresa = Item.OPT_Empresa.Empresa;
                response.Comuna = Item.OPT_Cliente.OPT_Comuna.Comuna;
                response.Region = Item.OPT_Cliente.OPT_Comuna.OPT_Region.Region;
                response.Direccion = Item.OPT_Cliente.Direccion;
                response.Fecha = Item.FechaIngreso;

                response.hipertension = Item.hipertension == true ? "SI" : "NO";
                response.Diabetes = Item.Diabetes == true ? "SI" : "NO";
                response.Alergias = Item.Alergias == true ? "SI" : "NO";
                response.Lentes = Item.Lentes == true ? "SI" : "NO";
                response.Observacion = Item.Observacion;


                return PartialView("_ModalDetalle", response);

            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }
        }
        #endregion     

    }
}