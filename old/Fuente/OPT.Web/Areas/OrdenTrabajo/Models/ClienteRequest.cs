using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class ClienteRequest
    {
        public long idOT { get; set; }
        public Nullable<int> idSucursal { get; set; }
        public Nullable<int> idEmpresa { get; set; }
        public string Beneficiario { get; set; }       
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
      

        public string RutCliente { get; set; }       
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public Nullable<int> idComuna { get; set; }
        public string Celular { get; set; }
        public string Mail { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string TipoPrevision { get; set; }
        public Nullable<System.DateTime> FechaIngreso { get; set; }

        public long idAtencion { get; set; } 

    }
}