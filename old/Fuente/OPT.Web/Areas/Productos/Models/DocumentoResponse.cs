using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Productos.Models
{
    public class DocumentoResponse
    {
        public DocumentoResponse()
        {
            Productos = new List<DetalleDocumento>();
        }

        public string NumDocumento { get; set; }
        public int IngresoEgreso { get; set; }
        public string Proveedor { get; set; }
        public string TipoDocumento { get; set; }
        public System.DateTime FechaDocumento { get; set; }
        public System.DateTime FechaIngreso { get; set; }

        public List<DetalleDocumento> Productos { get; set; }
    }

    public class DetalleDocumento
    {
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public int ValorUnitario { get; set; }

    }
}