using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.Flujo.Models
{
    public class EnviarFlujoResponse
    {
        public EnviarFlujoResponse()
        {
            ListEstados = new List<SelectListItem>();
        }

        public long idOt { get; set; }
        public DateTime FechaAntencion { get; set; }
        public string RutCliente { get; set; }
        public string NombreCliente { get; set; }
        public string Empresa { get; set; }
        public int idEstado { get; set; }
        public string Estado { get; set; }

        public List<SelectListItem> ListEstados { get; set; }
    }
}