using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models.Pago
{
    public class PagoRequest
    {
        public long idOT { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string TipoPago { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}