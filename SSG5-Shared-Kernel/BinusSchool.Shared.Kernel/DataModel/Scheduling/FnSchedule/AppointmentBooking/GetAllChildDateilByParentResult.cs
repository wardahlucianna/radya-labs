using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAllChildDateilByParentResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public string StudentName { get; set; }
        public string IdBinusan { get; set; }
        public string IdHomeroomStudent { get; set; }
    }

}
