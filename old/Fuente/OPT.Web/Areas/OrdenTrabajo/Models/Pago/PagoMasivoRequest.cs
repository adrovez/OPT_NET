using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models.Pago
{
    public class PagoMasivoRequest
    {
        public int idEmpresa { get; set; }
        public DateTime FechaPago { get; set; }
    }
}