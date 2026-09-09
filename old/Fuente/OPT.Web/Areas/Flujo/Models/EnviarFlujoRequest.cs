using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Flujo.Models
{
    public class EnviarFlujoRequest
    {
        public long idOT { get; set; }
        public int idEstado { get; set; }
        public string Observacion { get; set; }
    }
}