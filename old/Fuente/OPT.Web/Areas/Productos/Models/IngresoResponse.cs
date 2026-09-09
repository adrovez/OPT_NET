using System.Collections.Generic;
using System.Web.Mvc;

namespace OPT.Web.Areas.Productos.Models
{
    public class IngresoResponse
    {
        public IngresoResponse()
        {
            ListTipoDocumento = new List<SelectListItem>();
            ListProductos = new List<SelectListItem>();
        }

        public List<SelectListItem> ListTipoDocumento { get; set; }
        public List<SelectListItem> ListProductos { get; set; }
    }
}