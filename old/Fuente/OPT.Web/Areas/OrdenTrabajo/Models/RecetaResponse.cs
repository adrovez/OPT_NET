using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.OrdenTrabajo.Models
{
    public class RecetaResponse
    {
        public long idRecetaCristales { get; set; }
        public long idOT { get; set; }
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
        public bool CheckLejos { get; set; }
        public bool CheckCerca { get; set; }
        public Nullable<bool> CheckCristalesLaboratorio { get; set; }
        public Nullable<bool> CheckUrgente { get; set; }
    }
}