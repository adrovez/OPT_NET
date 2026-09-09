using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class AbonoResponse
    {
        public long idAbono { get; set; }
        public long idOT { get; set; }
        public int idFormaPago { get; set; }
        public string FormaPago { get; set; }
        public decimal Monto { get; set; }
        public System.DateTime FechaAbono { get; set; }

        
    }
}