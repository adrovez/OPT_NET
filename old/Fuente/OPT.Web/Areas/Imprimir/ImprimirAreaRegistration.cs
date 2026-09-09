using System.Web.Mvc;

namespace OPT.Web.Areas.Imprimir
{
    public class ImprimirAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Imprimir";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Imprimir_default",
                "Imprimir/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}