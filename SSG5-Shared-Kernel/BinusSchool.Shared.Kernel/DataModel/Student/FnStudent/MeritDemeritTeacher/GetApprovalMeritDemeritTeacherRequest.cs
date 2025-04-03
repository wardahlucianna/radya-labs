using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetApprovalMeritDemeritTeacherRequest : CollectionSchoolRequest
    {
        public string IdAcademiYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string Semester { get; set; }
        public string IdHomeroom { get; set; }
        public string Status { get; set; }
        public string IdUser { get; set; }
    }
}
