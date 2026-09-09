using OPT.Entidad;
using System.Collections.Generic;
using System.Web.Mvc;

namespace OPT.Web.Areas.Imprimir.Models
{
    public class ImprimirOtResponse
    {
        public ImprimirOtResponse()
        {
            recetaCristales = new OPT_RecetaCristales();
            ordenDeTrabajoDetalles = new List<OPT_OrdenDeTrabajoDetalle>();
        }

        public long OT { get; set; }
        public string FechaAtencion { get; set; }
        public string FechaEntrega { get; set; }
        public string HoraEntrega { get; set; }
        public string Telefono { get; set; }
        public string RutCliente { get; set; }
        public string NombreCliente { get; set; }
        public string EmpresaCliente { get; set; }
        public long Monto { get; set; }
        public long Abono { get; set; }
        public long Saldo { get; set; }
        public int Cuotas { get; set; }
        public long MontoCuota { get; set; }

        public OPT_RecetaCristales recetaCristales { get; set; }
        public List<OPT_OrdenDeTrabajoDetalle> ordenDeTrabajoDetalles { get; set; }
    }
}