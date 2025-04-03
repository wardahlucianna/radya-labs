using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Floor
{
    public class GetDetailFloorResult
    {
        public string IdFloor { get; set; }
        public string FloorName { get; set; }
        public ItemValueVm Building { get; set; }
        public bool HasLocker { get; set; }
        public string LockerTowerCodeName { get; set; }
        public string Description { get; set; }
        public string? URL { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public decimal? FileSize { get; set; }
        public bool? IsShowFloorLayout { get; set; }

    }
}
