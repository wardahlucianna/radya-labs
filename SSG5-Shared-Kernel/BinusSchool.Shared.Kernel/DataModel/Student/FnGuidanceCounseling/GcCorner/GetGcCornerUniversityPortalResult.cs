using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerUniversityPortalResult : ItemValueVm
    {
        public string UnivercityName { get; set; }
        public string UnivercityWebsite { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string CreatedBy { get; set; }
        public List<FactSheetGcCornerUniversityPortal> FactSheet { get; set; }
        public List<LogoGcCornerUniversityPortal> Logo { get; set; }
    }

    public class FactSheetGcCornerUniversityPortal
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
    }

    public class LogoGcCornerUniversityPortal
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
    }
}
