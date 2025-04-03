using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class DownloadSummaryCounselingServiceEntryResult
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public string StudentName { get; set; }
        public string IdBinusian { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm HomeRoom { get; set; }
        public NameValueVm CounselingCategory { get; set; }
        public string CounselorName { get; set; }
        public string CounselingWith { get; set; }
        public string RefredBy { get; set; }
        public List<NameValueVm> ConcernCategory { get; set; }
        public string BriefReport { get; set; }
        public string FollowUp { get; set; }
    }
}
