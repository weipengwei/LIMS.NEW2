﻿using System;
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
        private readonly UserService _userService= new UserService();

        public ActionResult Index()
        {
            ViewBag.ShowRoots = true;
            string id = this.UserContext.UserId;
            var mode = new UserService().Get(id);
            ViewBag.ShowRoots = false;
            ViewBag.Units = new UnitService().GetAllById(mode.UnitId);
            return View(mode);
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public JsonNetResult Save(UserModel user)
        {
            if (!this.Validate(user))
            {
                return JsonNet(new ResponseResult(false, "The required attributes of user are not filled.", ErrorCodes.RequireField));
            }
            if (_userService.GetByAccount(user.Account, null) != null)
            {
                return JsonNet(new ResponseResult(false, "该账号已存在！.", ErrorCodes.RequireField));
            }
            _userService.Save(new UserEntity
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

        /// <summary>
        /// 添加用户时验证用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
    }
}
