using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class GetStudentProgrammeHistoryRequest : CollectionSchoolRequest
    {
        public string IdStudent { get; set; }
    }
}
