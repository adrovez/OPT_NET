using OPT.Entidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.Anamnesis.Models.Ingresar
{
    public class AnamnesisCreateResponse
    {
        public AnamnesisCreateResponse()
        {
            Empresas = new List<SelectListItem>();
            Regiones = new List<SelectListItem>();
            Comunas = new List<SelectListItem>();
        }


        public long IdAnamnesis { get; set; }

        public OPT_Cliente Cliente { get; set; }
        public List<SelectListItem> Empresas { get; set; }
        public List<SelectListItem> Regiones { get; set; }
        public List<SelectListItem> Comunas { get; set; }

    }
}