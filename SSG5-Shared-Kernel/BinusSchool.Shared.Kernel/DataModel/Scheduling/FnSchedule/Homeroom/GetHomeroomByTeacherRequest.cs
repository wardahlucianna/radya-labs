using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomByTeacherRequest : CollectionSchoolRequest
    {
        public string IdAcademicyear { get; set; }
        public string IdTeacher { get; set; }
        public int? Semester { get; set; }
    }
}
