using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class SaveStudentExtracurricularRequest
    {
        public string IdStudent { get; set; }
        //public string IdGrade { get; set; }
        //public int Semester { get; set; }
        public string IdUserIn { get; set; }
        public List<SaveStudentExtracurricularRequest_Extracurricular> ExtracurricularList { get; set; }
    }

    public class SaveStudentExtracurricularRequest_Extracurricular
    {
        public string IdExtracurricular { get; set; }
        public bool IsChecked { get; set; }
    }
}
