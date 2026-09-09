using System.Collections.Generic;
using System.Web.Mvc;

namespace OPT.Web.Areas.OrdenTrabajo.Models.Pago
{
    public class PagoMasivoView
    {
        public PagoMasivoView()
        {
            Empresas = new List<SelectListItem>();           
        }

        public List<SelectListItem> Empresas { get; set; }
    }
}