using OPT.Dato.DAL;
using OPT.Entidad;
using OPT.Web.Areas.Clientes.Models;
using OPT.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.Clientes.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class FichaMedicaController : Controller
    {               
        public ActionResult Details(string id)
        {
            FichaMedicaResponse response = new FichaMedicaResponse();

            OPT_ClienteDAL clienteDAL = new OPT_ClienteDAL();
            OPT_AnamnesisDAL anamnesisDAL = new OPT_AnamnesisDAL();
            OPT_RecetaCristalesDAL recetaCristalesDAL = new OPT_RecetaCristalesDAL();
            OPT_OrdenDeTrabajoDAL ordenDeTrabajoDAL = new OPT_OrdenDeTrabajoDAL();
            OPT_EstadoOtDAL estadoOtDAL = new OPT_EstadoOtDAL();
            UtilController util = new UtilController();

            try
            {               
                response.Cliente = clienteDAL.Buscar(id);
                response.Anamnesis = anamnesisDAL.ListaPorRut(id);
                response.OrdenTrabajos = ordenDeTrabajoDAL.Lista(id);
                response.Estados = estadoOtDAL.Listar();
                response.Recetas = recetaCristalesDAL.Listar(id);

                if (response.Cliente.FechaNacimiento!=null)
                {
                    response.EdadCliente = util.CalcularEdad(DateTime.Parse(response.Cliente.FechaNacimiento.ToString()));
                }                  
                               
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(response);
        }


        #region ============ RECETA ============
        public PartialViewResult ModalCreateReceta(string id)
        {
            OPT_RecetaCristales response = new OPT_RecetaCristales();

            try
            {
                response.RutCliente = id;

                return PartialView("_ModalCreateReceta", response);
            }
            catch (Exception ex)
            {
                Libreria.LogError.setLogExeption(ex);
                throw ex;
            }


        }
        public PartialViewResult ModalReceta(long id)
        {
            OPT_RecetaCristales response = new OPT_RecetaCristales();
            OPT_RecetaCristalesDAL recetaCristalesDAL = new OPT_RecetaCristalesDAL();

            try
            {
                response = recetaCristalesDAL.Buscar(id);

                return PartialView("_ModalReceta", response);
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