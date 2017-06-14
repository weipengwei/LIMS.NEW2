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
        /// <summary>
        /// 要求检验的功能编号
        /// </summary>
        protected UnitType[] FunctionCodes
        {
            get;
            set;
        }

        public AdminActionFilterAttribute(params UnitType[] funcodes)
        {
            this.FunctionCodes = funcodes;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            CustomPrincipal customPrincipal = filterContext.HttpContext.User as CustomPrincipal;
            if (customPrincipal == null || customPrincipal.Context == null)
            {
                filterContext.Result= new ContentResult() { Content = "无权访问" };
            }
            if (!FunctionCodes.Contains(customPrincipal.Context.UnitType))
            {
                filterContext.Result = new ContentResult() { Content = "无权访问" };
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
