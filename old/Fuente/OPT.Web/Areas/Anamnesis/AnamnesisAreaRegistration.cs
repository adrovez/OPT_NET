using System.Web.Mvc;

namespace OPT.Web.Areas.Anamnesis
{
    public class AnamnesisAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Anamnesis";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Anamnesis_default",
                "Anamnesis/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}