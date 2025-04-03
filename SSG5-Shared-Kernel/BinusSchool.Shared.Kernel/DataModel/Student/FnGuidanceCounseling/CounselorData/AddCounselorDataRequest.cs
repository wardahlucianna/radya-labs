using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData
{
    public class AddCounselorDataRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string OfficerLocation { get; set; }
        public string ExtensionNumber { get; set; }
        public string OtherInformation { get; set; }
        public string IdRole { get; set; }
        public string IdPosition { get; set; }
        public List<GradeCounselorData> ListGradeCounselorData { get; set; }
        public List<AttachmentCounselorData> ListAttachmentCounselorData { get; set; }
    }

    public class GradeCounselorData
    {
        public string Id { get; set; }
        public string IdGrade { get; set; }
    }

    public class AttachmentCounselorData
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
