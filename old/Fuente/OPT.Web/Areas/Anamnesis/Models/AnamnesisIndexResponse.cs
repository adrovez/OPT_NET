using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Areas.Anamnesis.Models
{
    public class AnamnesisIndexResponse
    {
        public AnamnesisIndexResponse()
        {
            Empresas = new List<SelectListItem>();
            Anamnesis = new List<AnamnesisViewResponse>();
        }

        public List<AnamnesisViewResponse> Anamnesis { get; set; }
        public List<SelectListItem> Empresas { get; set; }
    }


}