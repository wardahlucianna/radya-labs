using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class GetListExemplaryCharacterSummaryResult
    {
        public int TotalStudent { get; set; }
        public int TotalExemplary { get; set; }
        public List<GetListExemplaryCharacterSummaryResult_Exemplary> ExemplaryList { get; set; }
    }

    public class GetListExemplaryCharacterSummaryResult_Exemplary
    {
        public NameValueVm Exemplary { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public List<ItemValueVm> Value { get; set; }
        public string Description { get; set; }
        public DateTime? ExemplaryDate { get; set; }
        public int LikesCount { get; set; }
        public NameValueVm PostedBy { get; set; }
        public NameValueVm ModifiedBy { get; set; }
        public bool PostedByMe { get; set; }
        public bool IsLikedByMe { get; set; }
        public List<GetListExemplaryCharacterSummaryResult_Attachment> Attachment { get; set; }
        public List<GetListExemplaryCharacterSummaryResult_ExemplaryStudent> StudentList { get; set; }
    }

    public class GetListExemplaryCharacterSummaryResult_ExemplaryStudent
    {
        public ItemValueVm Grade { get; set; }
        public NameValueVm Student { get; set; }
    }

    public class GetListExemplaryCharacterSummaryResult_Attachment
    {
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string Url { get; set; }
    }
}
