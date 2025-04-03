using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListStudentBySupervisorRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdUser { get; set; }
        public StatusOverallExperienceStudent? Status { get; set; }
    }
}
