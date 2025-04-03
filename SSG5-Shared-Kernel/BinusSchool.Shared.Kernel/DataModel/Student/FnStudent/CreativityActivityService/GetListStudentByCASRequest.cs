using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListStudentByCASRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string ViewAs { get; set; }
        public StatusOverallExperienceStudent? StatusOverallExperienceStudent { get; set; }
        public string IdUser { get; set; }
    }
}
