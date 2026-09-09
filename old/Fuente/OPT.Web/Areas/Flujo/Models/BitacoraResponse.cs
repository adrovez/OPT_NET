using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Flujo.Models
{
    public class BitacoraResponse
    {
        public long idBitacoraOT { get; set; }
        public long idOT { get; set; }
        public string Estado { get; set; }
        public System.DateTime Fecha { get; set; }
        public string Responsable { get; set; }
        public string Observacion { get; set; }
    }
}