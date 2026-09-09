using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models.OrdenTrabajo
{
    public class OrdenTrabajoDetalleResponse
    {

        public OrdenTrabajoDetalleResponse()
        {
            Abonos = new List<AbonoResponse>();
            Productos = new List<ProductoResponse>();
            Cristales = new OPT_RecetaCristales();
            Cliente = new OPT_Cliente();
        }

        public long idOT { get; set; }
        public string Beneficiario { get; set; }
        public string RutCliente { get; set; }
        //public string NombreCliente { get; set; }
        //public string FechaNacimiento { get; set; }
        //public string Comuna { get; set; }
        //public string Region { get; set; }
        //public string Direccion { get; set; }
        public DateTime FechaAtencion { get; set; }
        public DateTime FechaEntrega { get; set; }
        public TimeSpan HoraEntrega { get; set; }
        public Nullable<decimal> Precio { get; set; }
        public Nullable<decimal> Abono { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<int> NumeroCuota { get; set; }
        public string EstadoPago { get; set; }
        public string EtapaOT { get; set; }
        public Nullable<int> idSucursal { get; set; }
        public string Usuario { get; set; }
        public Nullable<int> idEmpresa { get; set; }
        public string NombreEmpresa { get; set; }

        public List<AbonoResponse> Abonos { get; set; }
        public List<ProductoResponse> Productos { get; set; }
        public OPT_RecetaCristales Cristales { get; set; }
        public OPT_Cliente Cliente { get; set; }
    }
}