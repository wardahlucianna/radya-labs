using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Org.BouncyCastle.Asn1.Ocsp;

namespace BinusSchool.Data.Model.School.FnSchool.Floor
{
    public class SaveFloorRequest
    {
        public string? IdFloor { get; set; }
        public string IdBuilding { get; set; }
        public string FloorName { get; set; }
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
