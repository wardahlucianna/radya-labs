using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailMeritTeacherV2Result
    {
        public string Student { get; set; }
        public string Homeroom { get; set; }
        public int TotalMerit { get; set; }
        public int TotalDemerit { get; set; }
        public string LevelOfInfraction { get; set; }
        public string Sanction { get; set; }
        public List<WizardStudentDetailMeritV2> Merit { get; set; }
    }
}
