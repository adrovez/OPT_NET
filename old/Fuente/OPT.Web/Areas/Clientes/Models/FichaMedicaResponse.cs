using OPT.Entidad;
using System.Collections.Generic;

namespace OPT.Web.Areas.Clientes.Models
{
    public class FichaMedicaResponse
    {
        public FichaMedicaResponse()
        {
            Cliente = new OPT_Cliente();
            Recetas = new List<OPT_RecetaCristales>();
            Anamnesis = new List<OPT_Anamnesis>();
            OrdenTrabajos = new List<OPT_OrdenDeTrabajo>();
            Estados = new List<OPT_EstadoOT>();
        }

        public int EdadCliente { get; set; }
        public OPT_Cliente Cliente { get; set; }
        public List<OPT_RecetaCristales> Recetas { get; set; }
        public List<OPT_Anamnesis> Anamnesis { get; set; }
        public List<OPT_OrdenDeTrabajo> OrdenTrabajos { get; set; }
        public List<OPT_EstadoOT> Estados { get; set; }
    }
}