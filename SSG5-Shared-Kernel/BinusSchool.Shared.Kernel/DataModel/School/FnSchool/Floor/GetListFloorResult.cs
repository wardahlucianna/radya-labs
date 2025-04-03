using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Floor
{
    public class GetListFloorResult
    {
        public string IdFloor { get; set; }
        public ItemValueVm Building { get; set; }
        public string FloorName { get; set; }
        public string Description { get; set; }
        public bool CanDelete { get; set; }

    }
}
