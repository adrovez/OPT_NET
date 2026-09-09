using System.Web.Mvc;

namespace OPT.Web.Areas.OrdenTrabajo
{
    public class OrdenTrabajoAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "OrdenTrabajo";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "OrdenTrabajo_default",
                "OrdenTrabajo/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}