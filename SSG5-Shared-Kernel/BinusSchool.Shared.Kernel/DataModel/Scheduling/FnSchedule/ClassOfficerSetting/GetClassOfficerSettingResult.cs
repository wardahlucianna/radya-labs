using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting
{
    public class GetClassOfficerSettingResult : ItemValueVm
    {
        public CodeWithIdVm IdAcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm HomeRoom { get; set; }
        public int? Semester { get; set; }
        public DateTime? LastModified { get; set; }
        public string? UserModified { get; set; }
        public string? UserIdModified { get; set; }
    }
}
