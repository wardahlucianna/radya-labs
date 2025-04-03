using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables
{
    public class AscTimetableGetListResult: CodeWithIdVm
    {
        public string Academicyears { get; set; }
        public string ScheduleName { get; set; }
        public string TotalParticipans { get; set; }
        public string SessionSetName { get; set; }
    }
}
