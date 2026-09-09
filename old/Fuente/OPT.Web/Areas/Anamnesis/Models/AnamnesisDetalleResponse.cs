using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Anamnesis.Models
{
    public class AnamnesisDetalleResponse
    {
        public long idAnamnesis { get; set; }
        public string RutCliente { get; set; }
        public string NombreCliente { get; set; }
        public string NombreEmpresa { get; set; }
        public string Comuna { get; set; }
        public string Region { get; set; }
        public string Direccion { get; set; }
        public DateTime Fecha { get; set; }       

        public string hipertension { get; set; }
        public string Diabetes { get; set; }
        public string Alergias { get; set; }
        public string Lentes { get; set; }
        public string Observacion { get; set; }


    }
}