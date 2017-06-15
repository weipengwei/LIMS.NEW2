using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIMS.Models
{
    public class MainMenusModel
    {
        public IList<MenuModel> Menus
        {
            get; set;
        }

        public bool IsAdmin
        {
            get; set;
        }

        /// <summary>
        /// 账号
        /// </summary>
        public string UserAccount
        {
            get; set;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get; set;
        }
    }
}
