using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPT.Web.Areas.Anamnesis.Models.Ingresar
{
    public class AnamnesisPreguntasRequest
    {
      
        public long idAnamnesis { get; set; }
        public int idEmpresa { get; set; }
        public string RutCliente { get; set; }      
        public int hipertension { get; set; }
        public int Diabetes { get; set; }
        public int Alergias { get; set; }
        public int Lentes { get; set; }
        public string Observacion { get; set; }

    }
}