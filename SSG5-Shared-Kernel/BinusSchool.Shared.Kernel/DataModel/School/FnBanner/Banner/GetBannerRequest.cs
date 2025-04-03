using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnBanner.Banner
{
    public class GetBannerRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public List<string> IdRoleGroup { get; set; }
        public List<string> IdLevel { get; set; }
        public List<string> IdGrade { get; set; }
        public List<BannerStatusFilter> Status { get; set; }
    }
}
