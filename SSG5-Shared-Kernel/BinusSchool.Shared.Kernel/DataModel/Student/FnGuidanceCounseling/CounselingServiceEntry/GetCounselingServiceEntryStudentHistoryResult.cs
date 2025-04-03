using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselingServiceEntryStudentHistoryResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public NameValueVm CounselingCategory { get; set; }
        public string CounselorName { get; set; }
        public DateTime CounselingDate { get; set; }
        public StudentData Students { get; set; }

    }
    public class StudentData 
    {
        public string StudentName { get; set; }
        public string IdBinusian { get; set; }
    }
}
