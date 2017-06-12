using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

using LIMS.Util;
using LIMS.Models;
using LIMS.Entities;
using LIMS.Services;
using LIMS.MVCFoundation.Controllers;
using LIMS.MVCFoundation.Core;
using LIMS.MVCFoundation.Helpers;

namespace LIMS.Web.Controllers
{
    /// <summary>
    /// 登录服务
    /// </summary>
    public class LogonController : Controller
    {
        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="logon"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Validate(LogonRequestModel logon)
        {
            UserEntity user;
            if (UserService.TryGetUserByAccount(logon.Account, out user)
                && SecurityHelper.ValidatePassword(logon.Password, user.Password))
            {
                SecurityHelper.CreateTicket(this.Response, user);

                return Json(new LogonResponseModel
                {
                    IsSuccess = true
                });
            }
            else
            {
                return Json(new ResponseResult(false, "账号或密码不存在或不匹配！"));
            }
        }

        ///// <summary>
        ///// 登录验证
        ///// </summary>
        ///// <param name="logon"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonNetResult Validate(LogonRequestModel logon)
        //{
        //    UserEntity user;
        //    if (UserService.TryGetUserByAccount(logon.Account, out user)
        //        && SecurityHelper.ValidatePassword(logon.Password, user.Password))
        //    {
        //        if (false)//user.IsChangePassword)
        //        {
        //            return JsonNet(new LogonResponseModel
        //            {
        //                RedirectUrl = this.Url.Content("~/Logon/ChangePassword")
        //            });
        //        }
        //        else
        //        {
        //            SecurityHelper.CreateTicket(this.Response, user);

        //            return JsonNet(new LogonResponseModel
        //            {
        //                IsSuccess = true,
        //                RedirectUrl = string.IsNullOrEmpty(logon.ReturnUrl) ? this.Url.Content("~/Main/Index") : logon.ReturnUrl
        //            });
        //        }
        //    }
        //    else
        //    {
        //        return JsonNet(new ResponseResult(false, "账号或密码不存在或不匹配！"));
        //    }
        //}
    }
}
