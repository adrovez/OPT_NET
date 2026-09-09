using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class FinalizaResponse
    {
        public long idOT { get; set; }
        public string Rut { get; set; }
        public string Cliente { get; set; }
        public string Empresa { get; set; }
    }
}