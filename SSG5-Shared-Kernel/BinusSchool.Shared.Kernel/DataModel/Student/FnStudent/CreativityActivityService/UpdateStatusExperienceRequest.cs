using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class UpdateStatusExperienceRequest
    {
        public string IdExperience { get; set; }
        public string IdStudent { get; set; }
        public ExperienceStatus ExperienceStatus { get; set; }
        public string Note { get; set; }

        //Kebutuhan Email
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
