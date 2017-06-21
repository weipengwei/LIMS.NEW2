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

namespace LIMS.Web.Controllers.Setting
{
    [RequiredLogon]
    [BaseEntityValue]
    public class UserController : BaseController
    {
        private readonly UserService _userService = new UserService();
        private readonly SystemPrivilegeService _systemPrivilegeService = new SystemPrivilegeService();
        private readonly UnitService _unitService = new UnitService();

        public ActionResult Index()
        {
            ViewBag.Roots = GetRoots();
            return View();
        }

        /// <summary>
        /// 下拉框选项
        /// </summary>
        /// <returns></returns>
        public JsonNetResult JsonIndex()
        {
            return JsonNet(new ResponseResult(true, JsonGetRoots()));
        }

        /// <summary>
        /// 列表页
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="rootId"></param>
        /// <param name="unitId"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public JsonNetResult JsonQuery(string condition, string rootId, string unitId, PagerInfo pager)
        {
            if (!this.IsAdmin)
            {
                condition = UserContext.RootUnitName;
            }
            var list = new UserService().Query(condition, rootId, unitId, pager);
            return JsonNet(new ResponseResult(true, list, pager));
        }

        public JsonNetResult Query(string condition, string rootId, string unitId, PagerInfo pager)
        {
            var list = new UserService().Query(condition, rootId, unitId, pager);
            return JsonNet(new ResponseResult(true, list, pager));
        }

        /// <summary>
        /// 编辑用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult JsonEdit(string id)
        {
            var mode = new UserService().Get(id);
            return JsonNet(new ResponseResult(true, mode));
        }

        public ActionResult Edit(string id)
        {
            ViewBag.ShowRoots = true;
            if (string.IsNullOrEmpty(id))
            {
                ViewBag.Roots = GetRoots();
                return View();
            }
            else
            {
                var mode = new UserService().Get(id);

                ViewBag.ShowRoots = false;
                ViewBag.Units = new UnitService().GetAllById(mode.UnitId);

                return View(mode);
            }
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public JsonNetResult Save(UserModel user)
        {
            if (!this.Validate(user))
            {
                return JsonNet(new ResponseResult(false, "The required attributes of user are not filled.", ErrorCodes.RequireField));
            }

            var service = new UserService();
            if (string.IsNullOrEmpty(user.Id))
            {
                var validationUser = service.GetByAccount(user.Account, user.Id);
                if (validationUser != null)
                {
                    return JsonNet(new ResponseResult(false, "账号重复了！"));
                }
            }
            service.Save(new UserEntity
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

        #region 私有方法

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

        private IDictionary<UnitType, List<object>> GetRoots()
        {
            var roots = new UnitService().GetByRootId(Constant.DEFAULT_UNIT_ROOT_ID);
            var hospitals = roots.Where(item => item.Type == UnitType.Hospital).Select(item =>
                new
                {
                    Id = item.Id,
                    Name = item.Name
                }).ToList<object>();
            var vendors = roots.Where(item => item.Type == UnitType.Vendor).Select(item =>
                new
                {
                    Id = item.Id,
                    Name = item.Name
                }).ToList<object>();

            var dic = new Dictionary<UnitType, List<object>>();
            dic[UnitType.Hospital] = hospitals;
            dic[UnitType.Vendor] = vendors;

            return dic;
        }

        private IDictionary<UnitType, List<object>> JsonGetRoots()
        {
            IList<UnitEntity> roots;
            if (this.IsAdmin)
            {
                roots = new UnitService().GetByRootId(Constant.DEFAULT_UNIT_ROOT_ID);
            }
            else
            {
                roots = new UnitService().GetByRootId(UserContext.RootUnitId);
            }
            var hospitals = roots.Where(item => item.Type == UnitType.Hospital).Select(item =>
                new
                {
                    Id = item.Id,
                    Name = item.Name
                }).ToList<object>();
            var vendors = roots.Where(item => item.Type == UnitType.Vendor).Select(item =>
                new
                {
                    Id = item.Id,
                    Name = item.Name
                }).ToList<object>();

            var dic = new Dictionary<UnitType, List<object>>();
            dic[UnitType.Hospital] = hospitals;
            dic[UnitType.Vendor] = vendors;

            return dic;
        }

        #endregion


    }
}
