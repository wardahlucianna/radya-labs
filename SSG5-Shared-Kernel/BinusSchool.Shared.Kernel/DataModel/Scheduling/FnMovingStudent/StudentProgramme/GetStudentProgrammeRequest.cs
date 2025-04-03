using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class GetStudentProgrammeRequest : CollectionSchoolRequest
    {
        public string idAcademicYear { get; set; }
        public string programme { get; set; }
    }
}
