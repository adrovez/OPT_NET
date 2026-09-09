using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Anamnesis.Models
{
    public class AnamnesisViewResponse
    {
        public long idAnamnesis { get; set; }
        public string RutCliente { get; set; }
        public string NombreCliente { get; set; }
        public string Empresa { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string RutUsuario { get; set; }
        public string Sucursal { get; set; }
    }
}