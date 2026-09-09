using OPT.Entidad.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPT.Web.Libreria
{
    //public class SessionExpireFilterAttribute : ActionFilterAttribute
    //{
    //    public override void OnActionExecuting(ActionExecutingContext filterContext)
    //    {
    //        HttpContext ctx = HttpContext.Current;
    //        // check if session is supported
    //        if (ctx.Session != null)
    //        {
    //            // check if a new session id was generated
    //            if (ctx.Session.IsNewSession)
    //            {
    //                if (filterContext.HttpContext.Request.IsAjaxRequest() && (!filterContext.HttpContext.Request.IsAuthenticated || filterContext.HttpContext.User == null))
    //                {                       
    //                    filterContext.RequestContext.HttpContext.Response.StatusCode = 401;
    //                }

    //                // If it says it is a new session, but an existing cookie exists, then it must // have timed out
    //                string sessionCookie = ctx.Request.Headers["OPT"];
    //                if ((null != sessionCookie) && (sessionCookie.IndexOf("ASP.NET_SessionId") >= 0))
    //                {
    //                    ctx.Response.Redirect("~/Home/LogOut");
    //                }
    //            }
    //        }
    //        base.OnActionExecuting(filterContext);
    //    }
    //}

    // Si no estamos logeado, regresamos al login

    public class AutenticadoAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (UsuarioConectado.GetExpiraSession())
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest() && (!filterContext.HttpContext.Request.IsAuthenticated || filterContext.HttpContext.User == null))
                {                   
                    filterContext.RequestContext.HttpContext.Response.StatusCode = 401;
                }
                else
                {                  
                    filterContext.Result = new System.Web.Mvc.RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new
                    {
                        controller = "Login",
                        action = "Index",
                        Areas = ""
                    }));
                }
            }
        }
    }

    // Si estamos logeado ya no podemos acceder a la página de Login
    public class NoLoginAttribute : System.Web.Mvc.ActionFilterAttribute
    {       
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!UsuarioConectado.GetExpiraSession())
            {               
                filterContext.Result = new System.Web.Mvc.RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Index",
                    Areas = ""

                }));
            }
            else
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {                   
                    filterContext.RequestContext.HttpContext.Response.StatusCode = 401;
                }               
            }
        }
    }


}