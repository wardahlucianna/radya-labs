using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnBanner.Banner
{
    public class AddBannerRequest
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public List<string> RoleGroupId { get; set; }
        public List<string> LevelId { get; set; }
        public List<string> GradeId { get; set; }
        public DateTime PublishStartDate { get; set; }
        public DateTime PublishEndDate { get; set; }
        public BannerOption Option { get; set; }
        public string Content { get; set; }
        public List<Attachment> Attachments { get; set; }
        public string Link { get; set; }
        public bool IsPin { get; set; }
        public string AcademicYear { get; set; }
        public string IdSchool { get; set; }
    }

    public class Attachment
    {
        public string OriginalFilename { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public bool IsImage { get; set; }
    }
}
