using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetSDRReligionReportDetailsResult
    {
        public ItemValueVm Student { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm HomeroomTeacher { get; set; }
        public ItemValueVm Streaming { get; set; }
        public string StudentReligion { get; set; }
        public ItemValueVm ReligionSubject { get; set; }
    }
}
