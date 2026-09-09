using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OPT.Web.Libreria
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeUserAttribute: AuthorizeAttribute
    {
        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    if (Session.Count == 0 || Session["CouncilID"] == null)
        //        Response.redirect("/Account/LogOn");

        //    if (Request.IsAjaxRequest() && (!Request.IsAuthenticated || User == null))
        //    {
        //        filterContext.RequestContext.HttpContext.Response.StatusCode = 401;
        //    }
        //    else
        //    {
        //        base.OnActionExecuting(filterContext);
        //    }
        //}

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.Request.IsAjaxRequest())
            {
                //validate http request.
                if (!httpContext.Request.IsAuthenticated || httpContext.Session["User"] == null) { 
                    FormsAuthentication.SignOut(); 
                    httpContext.Response.Redirect("~/?returnurl=" + httpContext.Request.Url.ToString()); 
                    return false; 
                } 
            } 
            return true; 
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new
                    { 
                        // put whatever data you want which will be sent 
                        // to the client
                        message = "sorry, but you were logged out" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet }; 
            } else 
            { 
                base.HandleUnauthorizedRequest(filterContext); 
            } 
        }
    }
}