using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LIMS.MVCFoundation.Core;
using LIMS.Util;

namespace LIMS.MVCFoundation.Attributes
{
    /// <summary>
    /// 管理员权限过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class AdminActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            CustomPrincipal customPrincipal = filterContext.HttpContext.User as CustomPrincipal;
            if (customPrincipal == null || customPrincipal.Context == null)
            {
                filterContext.Result= new ContentResult() { Content = "无权访问" };
            }
            if (customPrincipal.Context.UnitType != UnitType.Hospital && customPrincipal.Context.UnitType != UnitType.Vendor
                && customPrincipal.Context.UnitType != UnitType.Admin)
            {
                filterContext.Result = new ContentResult() { Content = "无权访问" };
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
