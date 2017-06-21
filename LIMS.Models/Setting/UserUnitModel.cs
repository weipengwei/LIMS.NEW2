using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LIMS.Entities;

namespace LIMS.Models.Setting
{
    public class UserUnitModel: UnitEntity
    {
        public bool Operate { get; set; }
    }
}
