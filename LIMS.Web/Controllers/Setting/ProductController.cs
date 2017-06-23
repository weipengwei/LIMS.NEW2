using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using LIMS.MVCFoundation.Core;
using LIMS.MVCFoundation.Controllers;
using LIMS.Services;
using LIMS.Entities;
using LIMS.Models;
using LIMS.Util;
using LIMS.MVCFoundation.Attributes;

namespace LIMS.Web.Controllers.Setting
{
    [RequiredLogon]
    [BaseEntityValue]
    public class ProductController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 产品分页查询
        /// </summary>
        /// <param name="condition">产品名称</param>
        /// <param name="pager">分页信息</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult Query(string condition, PagerInfo pager)
        {
            var list = new ProductService().Query(condition, pager);
             return JsonNet(new ResponseResult(true, list, pager));
        }

        /// <summary>
        /// 主键ID查询产品
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult JsonEdit(string id)
        {
            var mode = new ProductService().Get(id);
            return JsonNet(new ResponseResult(true, mode));
        }

        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View();
            }
            else
            {
                var mode = new ProductService().Get(id);

                return View(mode);
            }
        }

        /// <summary>
        /// 新增或修改产品（ID>0修改 ID=0新增）
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult Save(ProductEntity product)
        {
            if (!this.Validate(product))
            {
                return JsonNet(new ResponseResult(false, "The required attributes of product are not filled.", ErrorCodes.RequireField));
            }
            new ProductService().Save(product);
            return JsonNet(new ResponseResult());
        }

        /// <summary>
        /// 保存产品信息验证
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        private bool Validate(ProductEntity product)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                return false;
            }

            if (string.IsNullOrEmpty(product.MiniPackageUnit))
            {
                return false;
            }

            if (product.MiniPackageCount <= 0)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(product.RegisterNumber))
            {
                return false;
            }
            if (product.ValidDate < DateTime.Now)
            {
                return false;
            }
            return true;
        }
    }
}
