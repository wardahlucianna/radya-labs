using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselingServiceEntryResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public string StudentName { get; set; }
        public string IdBinusian { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm HomeRoom { get; set; }
        public NameValueVm CounselingCategory { get; set;}
        public string CounselorName { get; set; }
        public DateTime Date { get; set; }
    }
}
