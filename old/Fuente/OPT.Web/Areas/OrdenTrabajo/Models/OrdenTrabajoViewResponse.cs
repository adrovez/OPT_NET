using OPT.Entidad;
using System.Collections.Generic;
using System.Web.Mvc;


namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class OrdenTrabajoViewResponse
    {
        public OrdenTrabajoViewResponse()
        {
            Empresas = new List<SelectListItem>();
            Estados = new List<OPT_EstadoOT>();
        }

        public List<SelectListItem> Empresas { get; set; }
        public List<OPT_OrdenDeTrabajo> OrdenDeTrabajos { get; set; }
        public List<OPT_EstadoOT> Estados { get; set; }
    }
}