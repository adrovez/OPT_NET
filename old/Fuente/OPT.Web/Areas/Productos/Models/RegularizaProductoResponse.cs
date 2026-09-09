using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OPT.Entidad;

namespace OPT.Web.Areas.Productos.Models
{
    public class RegularizaProductoResponse
    {
        public RegularizaProductoResponse()
        {
            regularizaProducto = new List<OPT_RegularizaProducto>();
            producto = new OPT_Producto();
        }

        public List<OPT_RegularizaProducto> regularizaProducto { get; set; }
        public OPT_Producto producto { get; set; }
    }
}