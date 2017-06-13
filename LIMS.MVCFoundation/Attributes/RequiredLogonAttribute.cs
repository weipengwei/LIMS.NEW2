﻿using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using LIMS.MVCFoundation.Core;

namespace LIMS.MVCFoundation.Attributes
{
    /// <summary>
    ///  RequiredLogonAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class RequiredLogonAttribute : AuthorizeAttribute
    {
        ///// <summary>
        /////    HandleUnauthorizedRequest
        ///// </summary>
        ///// <param name="filterContext"></param>
        //protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        //{
        //    string result = SecurityRequestHandler.UnauthenticationRequestUrl(filterContext.RequestContext);

        //    if (filterContext.HttpContext.Request.IsAjaxRequest())
        //    {
        //        filterContext.HttpContext.Response.StatusCode = 401;
        //    }
        //    else
        //    {
        //        filterContext.HttpContext.Response.Redirect(result);
        //    }
        //    filterContext.HttpContext.Response.End();
        //}


        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool result = base.AuthorizeCore(httpContext);
            if (result) //校验此Ticket 是否在系统中存在
            {
                result = SecurityRequestHandler.IsLogon(httpContext.Request.RequestContext);
            }
            if (!httpContext.Response.Headers.AllKeys.Contains("LogonFlag"))
            {
                httpContext.Response.AddHeader("LogonFlag", result.ToString().ToLower());
            }

            return result;
        }


    }

    public class AllowCorsAttribute : ActionFilterAttribute
    {
        private string[] _domains;

        public AllowCorsAttribute(string domain)
        {
            _domains = new string[] {domain};
        }

        public AllowCorsAttribute(string[] domains)
        {
            _domains = domains;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = filterContext.RequestContext.HttpContext;
            var host = context.Request.UrlReferrer?.Host;
            if (host != null && _domains.Contains(host))
            {
                context.Response.AddHeader("Access-Control-Allow-Origin", $"http://{host}");
                context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}