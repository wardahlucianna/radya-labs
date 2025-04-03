using System.Collections.Generic;
using System;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnBanner.Banner
{
    public class GetBannerResult : ItemValueVm
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }
        public DateTime PublishStartDate { get; set; }
        public DateTime PublishEndDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModified { get; set; }
        public List<ViewRole> Role { get; set; }
        public List<ViewLevel> Level { get; set; }
        public List<ViewGrade> Grade { get; set; }
        public BannerStatus Status { get; set; }
        public string Link { get; set; }
        public bool IsPin { get; set; }
        public BannerOption BannerOption { get; set; }
        public string IdSchool { get; set; }
    }
}
