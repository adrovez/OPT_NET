using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class OrdenTrabajoRequest
    {
        public long idOT { get; set; }
        public int idEmpresa { get; set; }
        public Nullable<decimal> Precio { get; set; }
        public Nullable<decimal> Abono { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<int> NumeroCuota { get; set; }

        public Nullable<DateTime> InicioPago { get; set; }
    }
}