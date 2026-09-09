using System.Collections.Generic;
using System.Web.Mvc;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class ProductoResponse
    {
        public int idOT { get; set; }
        public string idProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Valor { get; set; }
        public string Producto { get; set; }
        public string Comentario { get; set; }

        public List<SelectListItem> ListProducto { get; set; }
    }
}