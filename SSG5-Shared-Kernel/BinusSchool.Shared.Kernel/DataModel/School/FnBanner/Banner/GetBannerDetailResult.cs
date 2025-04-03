using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnBanner.Banner
{
    public class GetBannerDetailResult : ItemValueVm
    {
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public DateTime PublishStartDate { get; set; }
        public DateTime PublishEndDate { get; set; }
        public List<ViewRole> Role { get; set; }
        public List<ViewLevel> Level { get; set; }
        public List<ViewGrade> Grade { get; set; }
        public BannerOption BannerOption { get; set; }
        public string Link { get; set; }
        public List<ViewAttachment> Attachments { get; set; }
        public AuditableResult Audit { get; set; }
        public bool IsPin { get; set; }
        public string Content { get; set; }
        public BannerStatus Status { get; set; }
        public string IdSchool { get; set; }
        public string NameSchool { get; set; }
    }

    public class ViewRole : CodeWithIdVm
    {
    }

    public class ViewLevel : CodeWithIdVm
    {
    }

    public class ViewGrade : CodeWithIdVm
    {
    }

    public class ViewAttachment : CodeWithIdVm
    {
        public string OriginalFilename { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public bool IsImage { get; set; }
    }
}
