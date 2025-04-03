using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class CreateActivityDataToMeritRequest
    {
        public string IdAcademicYear { get; set; }
        public List<ActivityMeritStudents> MeritStudents { get; set; }
    }

    public class ActivityMeritStudents
    {
        public string IdHomeroomStudent { get; set; }
        public string Note { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int Semeter { get; set; }
        public string IdMeritDemeritMapping { get; set; }
        public string NameMeritDemeritMapping { get; set; }
        public int Point { get; set; }
    }
}
