using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardStudentDetailMeritResult
    {
        public string Student { get; set; }
        public string Homeroom { get; set; }
        public int TotalMerit { get; set; }
        public int TotalDemerit { get; set; }
        public string LevelOfInfraction { get; set; }
        public string Sanction { get; set; }
        public List<WizardStudentDetailMerit> Merit { get; set; }
    }

    public class WizardStudentDetailMerit
    {
        public DateTime? Date { get; set; }
        public string DeciplineName { get; set; }
        public string Point { get; set; }
        public string Note { get; set; }
        public string Teacher { get; set; }
    }
}
