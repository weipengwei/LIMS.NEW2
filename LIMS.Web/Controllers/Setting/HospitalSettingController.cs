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
    public class HospitalSettingController : BaseController
    {
        public ActionResult List()
        {
            return View();
        }

        public ActionResult Edit(string id)
        {
            ViewBag.Title = "新增";
            if (!string.IsNullOrEmpty(id))
            {
                var hospital = new UnitService().Get(id);
                if (hospital != null)
                {
                    ViewBag.Title = "编辑";

                    return View(hospital);
                }
            }

            return View();
        }

        /// <summary>
        /// 院方列表查询页
        /// </summary>
        /// <param name="condition">医院名称</param>
        /// <param name="pager">分页信息</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult JsonQuery(string condition, PagerInfo pager)
        {
            if (!this.IsAdmin)
            {
                condition = new UnitService().Get(UserContext.RootUnitId)?.Name;
            }
            var list = new UnitService().QueryRoots(condition, UnitType.Hospital, pager);
            var result = new List<UnitModel>();
            foreach (var item in list)
            {
                result.Add(new UnitModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ShortCode = item.ShortCode,
                    ContactId = item.ContactId
                });
            }

            return JsonNet(new ResponseResult(true, result, pager));
        }

        /// <summary>
        /// 根据ID查询信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult QueryHospitalById(string id)
        {
            var hospital = new UnitService().Get(id);
            return Json(hospital);
        }

        /// <summary>
        /// 保存医院信息
        /// </summary>
        /// <param name="hospital"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult Save(UnitModel hospital)
        {
            //只有院方和管理员才可修改医院信息
            if (hospital == null)
            {
                throw new ArgumentNullException("The hospital is null.");
            }

            if (!this.Validate(hospital))
            {
                return JsonNet(new ResponseResult(false, "The required attributes of hospital are not filled.", ErrorCodes.RequireField));
            }
            if (!this.IsAdmin && UserContext.RootUnitId != hospital.Id)
            {
                return JsonNet(new ResponseResult(false, "只可修改本医院信息", ErrorCodes.RequireField));
            }

            new UnitService().Save(new UnitEntity
            {
                Id = hospital.Id,
                Name = hospital.Name,
                Type = UnitType.Hospital,
                Description = hospital.Description,
                ShortCode = hospital.ShortCode
            }, new ContactInfoEntity
            {
                Id = hospital.ContactInfo.Id,
                ContactPerson = hospital.ContactInfo.ContactPerson,
                Address = hospital.ContactInfo.Address,
                ContactWay1 = hospital.ContactInfo.ContactWay1,
                ContactWay2 = hospital.ContactInfo.ContactWay2,
                ContactWay3 = hospital.ContactInfo.ContactWay3,
                ContactWay4 = hospital.ContactInfo.ContactWay4
            });

            return JsonNet(new ResponseResult());
        }

        private bool Validate(UnitModel mode)
        {
            if (string.IsNullOrEmpty(mode.Name))
            {
                return false;
            }

            if (string.IsNullOrEmpty(mode.ContactInfo.ContactPerson))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 根据ID获取联系人信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetContactInfo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(null);
            }
            var mode = new ContactInfoService().Get(id);
            return Json(mode);
        }

        public ActionResult ContactInfo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(new ContactInfoModel());
            }
            else
            {
                var mode = new ContactInfoService().Get(id);
                if (mode == null)
                {
                    return View(new ContactInfoModel());
                }
                else
                {
                    return View(mode);
                }
            }
        }

        public ActionResult AuditingProducts(string hospitalId)
        {
            if (string.IsNullOrEmpty(hospitalId))
            {
                throw new Exception("The hospital id is empty.");
            }

            ViewBag.Context = new
            {
                HospitalId = hospitalId,
                Vendors = new UnitService().QueryRoots(UnitType.Vendor)
            };

            return View();
        }

        /// <summary>
        /// 医院产品产品审核页面
        /// </summary>
        /// <param name="hospitalId"></param>
        /// <returns></returns>
        public JsonNetResult JsonAuditingProducts(string hospitalId)
        {
            if (string.IsNullOrEmpty(hospitalId))
            {
                throw new Exception("The hospital id is empty.");
            }
            return JsonNet(new ResponseResult(true, new
            {
                HospitalId = hospitalId,
                Vendors = new UnitService().QueryRoots(UnitType.Vendor)
            }));
        }

        /// <summary>
        /// 根据供应商ID获取医院审核产品
        /// </summary>
        /// <param name="hospitalId">医院ID</param>
        /// <param name="vendorId">供应商ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult GetAuditingProducts(string hospitalId, string vendorId)
        {
            if (string.IsNullOrEmpty(hospitalId))
            {
                throw new Exception("The hospital id is empty.");
            }
            if (string.IsNullOrEmpty(vendorId))
            {
                throw new Exception("The vendor id is empty.");
            }

            try
            {
                var list = new AuditingProductService().QueryLatestVesion(hospitalId, vendorId).ToDictionary(item => item.ProductId);
                var products = new VendorProductService().QueryByVendor(vendorId);
                var productService = new ProductService();

                var result = new List<object>();
                foreach (var product in products)
                {
                    var productEntity = productService.Get(product.ProductId);
                    if (productEntity == null)
                    {
                        throw new Exception("The product does not exist.");
                    }

                    AuditingProductEntity auditEntity;
                    bool isAudit;
                    int version;
                    if (list.TryGetValue(product.ProductId, out auditEntity))
                    {
                        isAudit = auditEntity.IsAudit;
                        version = auditEntity.Version;
                    }
                    else
                    {
                        isAudit = false;
                        version = 0;
                    }

                    result.Add(new
                    {
                        ProductId = product.ProductId,
                        VendorId = vendorId,
                        Version = version,
                        Name = productEntity.Name,
                        IsAudit = isAudit
                    });
                }


                return JsonNet(new ResponseResult(true, result));
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(e));
            }
        }

        /// <summary>
        /// 保存审核结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult SaveAuditingProduct(AuditingProductEntity entity)
        {
            try
            {
                new AuditingProductService().Save(entity);
                return JsonNet(new ResponseResult());
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(false, e));
            }
        }

        /// <summary>
        /// 根据医院ID获取票据信息
        /// </summary>
        /// <param name="hospitalId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult JsonReceipts(string hospitalId)
        {
            if (string.IsNullOrWhiteSpace(hospitalId))
            {
                return JsonNet(new ResponseResult(false, null));
            }
            var list = new ReceiptInfoService().GetByHospital(hospitalId);
            return JsonNet(new ResponseResult(true, new
            {
                Receipts = list,
                hospitalId
            }));
        }

        public ActionResult Receipts(string hospitalId)
        {
            ViewBag.HospitalId = hospitalId;
            var list = new ReceiptInfoService().GetByHospital(hospitalId);

            return View(list);
        }

        /// <summary>
        /// 新增或修改票据信息
        /// </summary>
        /// <param name="receipt"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult SaveReceipt(ReceiptInfoModel receipt)
        {
            if (receipt == null)
            {
                throw new ArgumentNullException("The receipt is null.");
            }
            if (!this.ValidateReceipt(receipt))
            {
                return JsonNet(new ResponseResult(false, "The receipt is invalid.", ErrorCodes.RequireField));
            }
            var entity = new ReceiptInfoEntity
            {
                Id = receipt.Id,
                Title = receipt.Title,
                Tax = receipt.Tax,
                HospitalId = receipt.HospitalId
            };
            new ReceiptInfoService().Save(entity);
            receipt.Id = entity.Id;
            return JsonNet(new ResponseResult(true, receipt));
        }

        /// <summary>
        /// 验证发票信息
        /// </summary>
        /// <param name="receipt"></param>
        /// <returns></returns>
        [HttpPost]
        private bool ValidateReceipt(ReceiptInfoModel receipt)
        {
            if (string.IsNullOrEmpty(receipt.Title))
            {
                return false;
            }

            if (receipt.Tax < 0)
            {
                return false;
            }

            return true;
        }


       /// <summary>
       /// 获取所有医院的ID和名称
       /// </summary>
       /// <returns></returns>
       [HttpPost]
        public JsonNetResult JsonUnitList()
        {
            var hospitals = new UnitService().QueryRoots(UnitType.Hospital);
            return JsonNet(new ResponseResult(true, hospitals.Select(item => new { Id = item.Id, Name = item.Name })));
        }

        public ActionResult UnitList()
        {
            var hospitals = new UnitService().QueryRoots(UnitType.Hospital);

            ViewBag.Hospitals = hospitals.Select(item => new { Id = item.Id, Name = item.Name });
            return View();
        }

        /// <summary>
        /// 根据单位ID获取科室信息
        /// </summary>
        /// <param name="parentId">单位ID</param>
        /// <param name="condition">科室名称</param>
        /// <param name="pager"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult QueryUnits(string parentId, string condition, PagerInfo pager)
        {
            try
            {
                var service = new UnitService();
                var list = service.QueryUnits(condition, parentId, pager);

                var result = new List<dynamic>();
                var dic = GetHospitalReceipts(parentId);
                foreach (var item in list)
                {
                    ReceiptInfoModel receipt;
                    if (string.IsNullOrEmpty(item.DefaultReceiptId) || !dic.TryGetValue(item.DefaultReceiptId, out receipt))
                    {
                        receipt = null;
                    }

                    result.Add(new
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        ShortCode = item.ShortCode,
                        ContactId = item.ContactId,
                        ReceiptTitle = receipt == null ? "" : receipt.Title,
                        Tax = receipt == null ? 0 : receipt.Tax,
                        VendorName = service.Get(item.RootId).Name,
                        ParentId = item.ParentId,
                        BusinessTypeName = UnitBusinessTypeName.GetName(item.BusinessType)
                    });
                }

                return JsonNet(new ResponseResult(true, result, pager));
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(e));
            }
        }


        private IDictionary<string, ReceiptInfoModel> GetHospitalReceipts(string hospitalId)
        {
            var list = new ReceiptInfoService().GetByHospital(hospitalId);

            var dic = new Dictionary<string, ReceiptInfoModel>();
            foreach (var item in list)
            {
                dic[item.Id] = item;
            }

            return dic;
        }

        /// <summary>
        /// 获取医院票据信息
        /// </summary>
        /// <param name = "hospitalId" ></ param >
        /// < param name="id"></param>
        /// <returns></returns>
        public ActionResult UnitEdit(string hospitalId, string id)
        {
            ViewBag.HospitalId = hospitalId;
            ViewBag.HospitalReceipts = new ReceiptInfoService().GetByHospital(hospitalId);
            if (!string.IsNullOrEmpty(id))
            {
                var unit = new UnitService().Get(id);
                return View(unit);
            }

            return View();
        }


        /// <summary>
        /// 保存医院单位信息
        /// </summary>
        /// <param name="hospitalUnit"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult SaveHospitalUnit(UnitModel hospitalUnit)
        {
            if (hospitalUnit == null)
            {
                throw new ArgumentNullException("The hospital unit is null.");
            }

            if (!this.ValidateHospital(hospitalUnit))
            {
                return JsonNet(new ResponseResult(false, "The required attributes of hospital unit are not filled.", ErrorCodes.RequireField));
            }

            new UnitService().Save(new UnitEntity
            {
                Id = hospitalUnit.Id,
                Name = hospitalUnit.Name,
                Type = UnitType.HospitalUnit,
                Description = hospitalUnit.Description,
                ShortCode = hospitalUnit.ShortCode,
                DefaultReceiptId = hospitalUnit.ReceiptId,
                ParentId = hospitalUnit.ParentId,
                RootId = hospitalUnit.ParentId,
                BusinessType = hospitalUnit.BusinessType
            }, new ContactInfoEntity
            {
                Id = hospitalUnit.ContactInfo.Id,
                ContactPerson = hospitalUnit.ContactInfo.ContactPerson,
                Address = hospitalUnit.ContactInfo.Address,
                ContactWay1 = hospitalUnit.ContactInfo.ContactWay1,
                ContactWay2 = hospitalUnit.ContactInfo.ContactWay2,
                ContactWay3 = hospitalUnit.ContactInfo.ContactWay3,
                ContactWay4 = hospitalUnit.ContactInfo.ContactWay4
            });

            return JsonNet(new ResponseResult());
        }

        /// <summary>
        /// 验证医院单位信息
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [HttpPost]
        private bool ValidateHospital(UnitModel mode)
        {
            if (string.IsNullOrEmpty(mode.Name))
            {
                return false;
            }

            if (string.IsNullOrEmpty(mode.ReceiptId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(mode.ContactInfo.ContactPerson))
            {
                return false;
            }

            return true;
        }


        #region Hospital Products

        public ActionResult HospitalProducts(string hospitalId)
        {
            if (string.IsNullOrEmpty(hospitalId))
            {
                throw new Exception("The hospital id is empty.");
            }

            ViewBag.Context = new
            {
                HospitalId = hospitalId,
                Units = new UnitService().GetByRootId(hospitalId).Select(item => new { Id = item.Id, Name = item.Name }),
                Products = new ProductService().Query().Select(item => new { Id = item.Id, Name = item.Name, MiniPackageCount = item.MiniPackageCount, Category = item.Category })
            };

            return View();
        }

        public JsonNetResult GetHospitalProduct(string unitId, string productId)
        {
            try
            {
                var entity = new HospitalProductService().Get(unitId, productId);
                return JsonNet(new ResponseResult(true, entity));
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(e));
            }
        }

        /// <summary>
        /// 医院申请产品
        /// </summary>
        /// <param name="hospitalId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult JsonHospitalProducts(string hospitalId)
        {
            if (string.IsNullOrEmpty(hospitalId))
            {
                throw new Exception("The hospital id is empty.");
            }
            return JsonNet(new ResponseResult(true, new
            {
                HospitalId = hospitalId,
                Units = new UnitService().GetByRootId(hospitalId).Select(item => new { Id = item.Id, Name = item.Name }),
                Products = new ProductService().Query().Select(item => new { Id = item.Id, Name = item.Name, MiniPackageCount = item.MiniPackageCount, Category = item.Category })
            }));
        }

        /// <summary>
        /// 获取医院产品信息
        /// </summary>
        /// <param name="unitId">科室ID</param>
        /// <param name="productId">产品ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult JsonGetHospitalProduct(string unitId, string productId)
        {
            try
            {
                var IsUsing = false;
                var entity = new HospitalProductService().Get(unitId, productId);
                var product = new ProductService().GetByHospital(productId);
                return JsonNet(new ResponseResult(true, new
                {
                    entity= entity,
                    product= product,
                    IsUsing= entity==null
                }));
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(e));
            }
        }

        /// <summary>
        /// 保存医院申请产品
        /// </summary>
        /// <param name="hospitalProduct"></param>
        /// <returns></returns>
        public JsonNetResult SaveHospitalProduct(HospitalProductEntity hospitalProduct)
        {
            if (hospitalProduct == null)
            {
                return JsonNet(new ResponseResult(false,"请填写完整的信息"));
            }
            try
            {
                new HospitalProductService().Save(hospitalProduct);

                return JsonNet(new ResponseResult(true, hospitalProduct));
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(e));
            }
        }
        #endregion


        #region Form Approve List

        public ActionResult FormApproveList(string hospitalId)
        {
            this.ViewBag.Context = new
            {
                HospitalId = hospitalId,
                AuditForms = FormHelper.AuditForms()
            };

            return View();
        }

        /// <summary>
        /// 获取审核类型
        /// </summary>
        /// <param name="hospitalId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult JsonFormApproveList(string hospitalId)
        {
            return JsonNet(new ResponseResult(true, new
            {
                HospitalId = hospitalId,
                AuditForms = FormHelper.AuditForms()
            }));
        }

        /// <summary>
        /// 获取审核人员
        /// </summary>
        /// <param name="formType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult GetFormApproveList(FormType formType)
        {
            try
            {
                if (formType == FormType.None)
                {
                    throw new Exception("The audit form is not being chosen.");
                }

                var list = new FormApproveListService().Get(formType);

                var unitService = new UnitService();
                var userService = new UserService();

                var result = new List<dynamic>();
                var index = 1;
                foreach (var item in list)
                {
                    if (item.ApproverType == ApproverType.ChoosePerson)
                    {
                        var person = userService.Get(item.ApproverId);
                        result.Add(new
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            Type = new
                            {
                                Value = item.ApproverType,
                                Name = "指定人员"
                            },
                            Approver = new
                            {
                                Id = item.ApproverId,
                                Name = person == null ? string.Empty : person.Name
                            },
                            Sequence = index
                        });
                    }
                    else if (item.ApproverType == ApproverType.ChooseUnitManager)
                    {
                        var unit = unitService.Get(item.ApproverId);
                        result.Add(new
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            Type = new
                            {
                                Value = item.ApproverType,
                                Name = "部门主管"
                            },
                            Approver = new
                            {
                                Id = item.ApproverId,
                                Name = unit == null ? string.Empty : unit.Name
                            },
                            Sequence = index
                        });
                    }
                    else
                    {
                        result.Add(new
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            Type = new
                            {
                                Value = item.ApproverType,
                                Name = item.ApproverType == ApproverType.Applyer ? "申请人" : "申请部门主管"
                            },
                            Sequence = index
                        });
                    }

                    index++;
                }

                return JsonNet(new ResponseResult(true, result));
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(false, e));
            }
        }

        /// <summary>
        /// 保存审核配置
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult SaveFormApprove(FormApproveListEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new Exception("The form approver is empty.");
                }

                if (entity.FormType == FormType.None)
                {
                    throw new Exception("The audit form is not being chosen.");
                }

                if (entity.ApproverType == ApproverType.None)
                {
                    throw new Exception("The approver type is not being chosen.");
                }

                if ((entity.ApproverType == ApproverType.ChoosePerson || entity.ApproverType == ApproverType.ChooseUnitManager) && string.IsNullOrEmpty(entity.ApproverId))
                {
                    throw new Exception("The approver is empty.");
                }

                entity.Id = Guid.NewGuid().ToString();
                entity.CreatedId = entity.UpdatedId = this.UserContext.UserId;
                entity.CreatedTime = entity.UpdatedTime = DateTime.Now;

                new FormApproveListService().Save(entity);

                return JsonNet(new ResponseResult(true, entity.Id));
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(false, e));
            }
        }

       /// <summary>
       /// 删除审核配置
       /// </summary>
       /// <param name="id"></param>
       /// <returns></returns>
       [HttpPost]
        public JsonNetResult DeleteFormApprove(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("The approver id is empty.");
                }

                new FormApproveListService().Delete(id);

                return JsonNet(new ResponseResult());
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(false, e));
            }
        }

        /// <summary>
        /// 修改审核配置
        /// </summary>
        /// <param name="current"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult MoveFormApprove(string current, string exchange)
        {
            try
            {
                if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(exchange))
                {
                    throw new Exception("The one of exchange approvers a empty.");
                }

                new FormApproveListService().Move(current, exchange, this.UserContext.UserId);

                return JsonNet(new ResponseResult());
            }
            catch (Exception e)
            {
                return JsonNet(new ResponseResult(false, e));
            }
        }
        #endregion
    }
}
;