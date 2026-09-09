using OPT.Dato.DAL;
using OPT.Web.Areas.Imprimir.Models;
using OPT.Web.Libreria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.Imprimir.Controllers
{
    [Libreria.Autenticado]
    [Authorize]
    public class TicketController : Controller
    {
        // GET: Imprimir/Ticket
        public PartialViewResult TicketOT(int id)
        {
            OPT_OrdenDeTrabajoDAL DatoOT = new OPT_OrdenDeTrabajoDAL();
            OPT_OrdenDeTrabajoDetalleDAL ordenDeTrabajoDetalleDAL = new OPT_OrdenDeTrabajoDetalleDAL();
            OPT_RecetaCristalesDAL recetaCristalesDAL = new OPT_RecetaCristalesDAL();
            ImprimirOtResponse response = new ImprimirOtResponse();

            var Item = DatoOT.Buscar(id);  

            response.OT = Item.idOT;
            response.EmpresaCliente = Item.OPT_Empresa.Empresa;
            response.RutCliente = Item.OPT_Cliente.RutCliente;
            response.NombreCliente = Item.OPT_Cliente.Nombre;
            response.FechaAtencion = DateTime.Parse(Item.FechaAtencion.ToString()).ToShortDateString();
            response.FechaEntrega = DateTime.Parse(Item.FechaEntrega.ToString()).ToShortDateString();
            response.HoraEntrega = Item.HoraEntrega.ToString();
            response.Telefono = Item.OPT_Cliente.Celular;
           
            response.Monto = long.Parse(Item.Precio.ToString());
            response.Abono = long.Parse(Item.Abono.ToString());
            response.Saldo = long.Parse(Item.Saldo.ToString());

            response.Cuotas = Item.NumeroCuota ==null? 0 : int.Parse(Item.NumeroCuota.ToString());

            if(response.Cuotas > 0)
            {
                response.MontoCuota = (response.Saldo / response.Cuotas);
            }

            response.recetaCristales = Item.OPT_RecetaCristales.FirstOrDefault();
            response.ordenDeTrabajoDetalles = ordenDeTrabajoDetalleDAL.Lista(id);

            return PartialView("_ParcialTicketOT", response);
        }

        public void Imprimir(int id)
        {
            CrearTicket crearTicket = new CrearTicket();

            crearTicket.EncabezadoVenta();
        }

      
    }
}
