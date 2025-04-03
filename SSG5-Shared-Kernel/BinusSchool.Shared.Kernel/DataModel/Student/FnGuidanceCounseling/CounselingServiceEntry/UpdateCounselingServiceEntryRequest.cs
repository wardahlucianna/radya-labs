using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class UpdateCounselingServiceEntryRequest
    {
        public string Id { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdCounselor { get; set; }
        public string IdStudent { get; set; }
        public string CounselingWith { get; set; }
        public string IdCounselingCategory { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string ReferredBy { get; set; }
        public string BriefReport { get; set; }
        public string FollowUp { get; set; }
        public List<string> ConcernCategory { get; set; }
        public List<UpdateAttachment> Attachements { get; set; }
    }
    public class UpdateAttachment
    {
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
