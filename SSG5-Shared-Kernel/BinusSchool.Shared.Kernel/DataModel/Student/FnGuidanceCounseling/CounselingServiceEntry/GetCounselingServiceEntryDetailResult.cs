using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselingServiceEntryDetailResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public NameValueVm Counselor { get; set; }

        public NameValueVm CounselingCategory { get; set; }
        public DateTime? CounselingDate { get; set; }
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm HomeRoom { get; set; }
        public string ReferredBy { get; set; }
        public string BriefReport { get; set; }
        public string FollowUp { get; set; }
        public CounselingWith CounselingWith { get; set; }
        public string PictureStudent { get; set; }
        public List<NameValueVm> ConcernCategory { get; set; }
        public List<StudentParent> Parents { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
    public class StudentParent
    {
        public string IdParent{ get; set; }
        public string IdBinusian{ get; set; }
        public string ParentName { get; set; }
    }

    public class Attachment
    {
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
