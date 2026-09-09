using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class AbonoRequest
    {
        public long idAbono { get; set; }
        public long idOT { get; set; }
        public int idFormaPago { get; set; }
        public decimal Monto { get; set; }
        public System.DateTime FechaAbono { get; set; }
    }
}