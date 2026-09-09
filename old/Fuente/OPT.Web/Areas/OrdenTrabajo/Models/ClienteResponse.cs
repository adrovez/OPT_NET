using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class ClienteResponse
    {
        public ClienteResponse()
        {
            ListRegion = new List<SelectListItem>();
            ListComuna = new List<SelectListItem>();
            ListEmpresa = new List<SelectListItem>();
        }

        public string RutCliente { get; set; }
        public string Beneficiario { get; set; }
        public Nullable<int> idEmpresa { get; set; }
        public string Empresa { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public Nullable<int> idComuna { get; set; }
        public string Comuna { get; set; }
        public Nullable<int> idRegion { get; set; }
        public string Region { get; set; }
        public string Celular { get; set; }
        public string Mail { get; set; }
        public Nullable<System.DateTime> FechaIngreso { get; set; }
        public Nullable<System.DateTime> FechaNacimiento { get; set; }
        public string TipoPrevision { get; set; }

        public List<SelectListItem> ListRegion { get; set; }
        public List<SelectListItem> ListComuna { get; set; }
        public List<SelectListItem> ListEmpresa { get; set; }
    }
}