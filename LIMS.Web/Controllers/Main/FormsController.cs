using System.Web.Mvc;
using LIMS.MVCFoundation.Attributes;
using LIMS.MVCFoundation.Controllers;

namespace LIMS.Web.Controllers.Main
{
    [RequiredLogon]
    [BaseEntityValue]
    public class FormsController : BaseController
    {

        public ActionResult OrderView(bool? needApprove, bool? adjustPrice, bool? editPrice)
        {
            ViewBag.NeedApprove = needApprove.HasValue ? needApprove.Value : false;
            ViewBag.AdjustPrice = adjustPrice.HasValue ? adjustPrice.Value : false;
            ViewBag.EditPrice = editPrice.HasValue ? editPrice.Value : false;

            return View();
        }
        

        public ActionResult DeliveryEdit()
        {
            return View();
        }

        public ActionResult DeliveryView()
        {
            return View();
        }


        
    }
}