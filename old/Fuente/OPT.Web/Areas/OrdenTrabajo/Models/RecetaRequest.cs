using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class RecetaRequest
    {
        public long idRecetaCristales { get; set; }
        public long idOT { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string RutCliente { get; set; }
        public string LejosODEsferico { get; set; }
        public string LejosODCilindro { get; set; }
        public string LejosODEje { get; set; }
        public string LejosODObservacion { get; set; }
        public string LejosOIEsferico { get; set; }
        public string LejosOICilindro { get; set; }
        public string LejosOIEje { get; set; }
        public string LejosOIObservacion { get; set; }
        public string LejosDPEsferico { get; set; }
        public string LejosDPObservacion { get; set; }
        public string CercaODEsferico { get; set; }
        public string CercaODCilindro { get; set; }
        public string CercaODEje { get; set; }
        public string CercaODObservacion { get; set; }
        public string CercaOIEsferico { get; set; }
        public string CercaOICilindro { get; set; }
        public string CercaOIEje { get; set; }
        public string CercaOIObservacion { get; set; }
        public string LejosADDEsfera { get; set; }
        public string CercaDPEsferico { get; set; }
        public string CercaDPObservacion { get; set; }
        public int CheckLejos { get; set; }
        public int CheckCerca { get; set; }
        public int CheckCristalesLaboratorio { get; set; }
        public int CheckUrgente { get; set; }
    }
}