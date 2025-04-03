using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerYourCounselorResult : ItemValueVm
    {
        public string Name { get; set; }

        public CodeWithIdVm AcademicYear { get; set; }

        public string OfficeLocation { get; set; }

        public string ExtensionNumber { get; set; }

        public string Email { get; set; }

        public string OtherInformation { get; set; }

        public List<PhotoGcCournerCounsellor> Photo { get; set; }
    }

    public class PhotoGcCournerCounsellor
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
