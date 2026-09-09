using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Clientes.Models
{
    public class AtencionReporteRequest
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
    }
}