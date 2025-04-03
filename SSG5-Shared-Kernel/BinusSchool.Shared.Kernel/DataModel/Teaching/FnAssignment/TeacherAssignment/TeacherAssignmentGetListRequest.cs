using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment
{
    public class TeacherAssignmentGetListRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdClass { get; set; }
        public string Status { get; set; }
    }
}
