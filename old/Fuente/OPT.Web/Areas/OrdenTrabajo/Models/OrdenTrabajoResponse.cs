using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class OrdenTrabajoResponse
    {
        public OrdenTrabajoResponse()
        {
            cliente = new ClienteResponse();
            receta = new RecetaResponse();
            abonos = new List<AbonoResponse>();
            producto = new ProductoResponse();
            ListFormaPago = new List<SelectListItem>();
            detalleOT = new List<OPT_OrdenDeTrabajoDetalle>();
        }

        public long idOT { get; set; }
        public long idAnamnesis { get; set; }
        public Nullable<int> idSucursal { get; set; }
        public Nullable<int> idEmpresa { get; set; }
        public string Beneficiario { get; set; }
        public string RutCliente { get; set; }
        public Nullable<System.DateTime> FechaAtencion { get; set; }
        public Nullable<System.DateTime> FechaEntrega { get; set; }
        public Nullable<System.TimeSpan> HoraEntrega { get; set; }
        public Nullable<decimal> Precio { get; set; }
        public Nullable<decimal> Abono { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<int> NumeroCuota { get; set; }
        public string Estado { get; set; }
        public string Usuario { get; set; }
        public Nullable<int> EstadoPago { get; set; }
        public long idAtencion { get; set; }


        public List<SelectListItem> ListFormaPago { get; set; }
        public ClienteResponse cliente { get; set; }
        public RecetaResponse receta { get; set; }
        public List<AbonoResponse> abonos { get; set; }
        public ProductoResponse producto { get; set; }
        public List<OPT_OrdenDeTrabajoDetalle> detalleOT { get; set; }

}
}