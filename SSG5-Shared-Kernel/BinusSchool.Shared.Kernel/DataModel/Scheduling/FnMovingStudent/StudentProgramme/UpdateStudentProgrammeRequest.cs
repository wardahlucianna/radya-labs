using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class UpdateStudentProgrammeRequest
    {
        public string id { get; set; }
        public string homeroom { get; set; }
        public DateTime effectiveDate{ get; set; }
        public bool isSendEmail{ get; set; }
        public StudentProgrammeEnum Programme { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
