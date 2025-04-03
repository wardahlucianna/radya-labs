using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetClassIDByGradeAndStudentRequest
    {
        public string IdAscTimetable { get; set; }
        public string IdGrade { get; set; }
        public string IdStudent { get; set; }
        public string ClassID { get; set; }
    }
}
