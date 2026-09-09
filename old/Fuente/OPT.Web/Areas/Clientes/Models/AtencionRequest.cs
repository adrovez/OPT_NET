using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OPT.Web.Areas.OrdenTrabajo.Models;
using OPT.Web.Areas.Anamnesis.Models.Ingresar;

namespace OPT.Web.Areas.Clientes.Models
{
    public class AtencionRequest
    {
        public AtencionRequest()
        {
            cliente = new ClienteRequest();
            receta = new RecetaRequest();
            anamnesis = new AnamnesisPreguntasRequest();
        }
        public ClienteRequest cliente { get; set; }
        public RecetaRequest receta  { get; set;}
        public AnamnesisPreguntasRequest anamnesis { get; set; }       

    }
}