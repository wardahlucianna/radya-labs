using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class UpdateOverallProgressStatusStudentRequest
    {
        public List<string> IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public StatusOverallExperienceStudent StatusOverallExperienceStudent { get; set; }

        //kebutuhan email
        public string IdUser { get; set; }
    }
}
