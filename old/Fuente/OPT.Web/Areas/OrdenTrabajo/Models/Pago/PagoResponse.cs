using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace OPT.Web.Areas.OrdenTrabajo.Models.Pago
{
    public class PagoResponse
    {
        public PagoResponse()
        {
            Pagos = new List<OPT_Pago>();
            ListFormaPago = new List<SelectListItem>();
        }
        public long idOT { get; set; }
        public string Beneficiario { get; set; }
        public string RutCliente { get; set; }
        public string NombreCliente { get; set; }
        public Nullable<System.DateTime> FechaAtencion { get; set; }
        public decimal Precio { get; set; }
        public decimal Abono { get; set; }
        public decimal Saldo { get; set; }
        public string EstadoPago { get; set; }

        public List<OPT_Pago> Pagos { get; set; }
        public List<SelectListItem> ListFormaPago { get; set; }
    }
}