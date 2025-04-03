using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class SaveOverallStatusExperienceRequest
    {
        public string IdAcademicYear { get; set; }  
        public string IdStudent { get; set; }
        public string IdServiceAsActionStatus { get; set; }
    }
}
