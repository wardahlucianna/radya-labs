using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetWizardStudentAchievementResult
    {
        public string IdAcademicYear { get; set; }  
        public string IdHomeroomStudent { get; set; }  
        public string Semester { get; set; }  

        public List<GetWizardStudentAchievement> WizardStudentAchievement { get; set; }
    }

    public class GetWizardStudentAchievement
    {
        public DateTime? Date { get; set; }
        public string MeritDemerit { get; set; }
    }
}
