using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using LIMS.MVCFoundation.Core;
using LIMS.MVCFoundation.Controllers;
using LIMS.MVCFoundation.Helpers;
using LIMS.Services;
using LIMS.Entities;
using LIMS.Models;
using LIMS.Util;
using LIMS.MVCFoundation.Attributes;

namespace LIMS.Web.Controllers.Profile
{
    /// <summary>
    /// 用户以及权限
    /// </summary>
    [RequiredLogon]
    [BaseEntityValue]
    public class UserInfoController : BaseController
    {
        public JsonNetResult Save(UserModel user)
        {
            if (!this.Validate(user))
            {
                return JsonNet(new ResponseResult(false, "The required attributes of user are not filled.", ErrorCodes.RequireField));
            }

            new UserService().Save(new UserEntity
            {
                Id = user.Id,
                Name = user.Name,
                Account = user.Account,
                Password = string.IsNullOrEmpty(user.Password) ? string.Empty : SecurityHelper.HashPassword(user.Password),
                Title = user.Title,
                UnitId = user.UnitId,
                IsChangePassword = true
            });

            return JsonNet(new ResponseResult());
        }

        private bool Validate(UserModel user)
        {
            if (string.IsNullOrEmpty(user.Name))
            {
                return false;
            }

            if (string.IsNullOrEmpty(user.Account))
            {
                return false;
            }

            if (string.IsNullOrEmpty(user.Id) && (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.ValidPassword)))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(user.Password) && string.IsNullOrEmpty(user.ValidPassword))
            {
                if (string.Compare(user.Password, user.ValidPassword) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult UserPrivilege()
        {
            var user = new UserService().Get(UserContext.UserId);
            var unit = new UnitService().GetAllById(user.UnitId).FirstOrDefault();

            return JsonNet(new ResponseResult(true, new
            {
                user = new
                {
                    user_Id = user.Id,
                    user_Account = user.Account
                },
                unit = new
                {
                    unit_Type = unit.Type.GetHashCode(),
                    unit_ParentId = unit.ParentId,
                }
            }));
        }
    }
}
