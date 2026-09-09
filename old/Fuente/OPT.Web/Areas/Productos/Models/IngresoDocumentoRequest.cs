using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Productos.Models
{
    public class IngresoDocumentoRequest
    {
        public long idProductoDocumento { get; set; }
        public int idTipoDocumento { get; set; }
        public string NumDocumento { get; set; }
        public int IngresoEgreso { get; set; }
        public string Proveedor { get; set; }
        public DateTime FechaDocumento { get; set; }
        public DateTime FechaIngreso { get; set; }
        public List<Producto> Productos { get; set; }
    }

    public class Producto
    {
        public long idProducto { get; set; }
        public int Cantidad { get; set; }
        public int ValorUnitario { get; set; }
    }


}