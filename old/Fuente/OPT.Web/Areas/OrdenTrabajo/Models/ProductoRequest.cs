using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class ProductoRequest
    {
        public int idOT { get; set; }
        public long idProducto { get; set; }
        public int Cantidad { get; set; }
        public string Valor { get; set; }
        public string Producto { get; set; }
        public string Comentario { get; set; }
    }
}