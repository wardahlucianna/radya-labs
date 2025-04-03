using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.LHAndCA
{
    public class AddAssignLHAndCARequest
    {
        public string IdSchool {get;set;}
        public string IdAcademicYear {get;set;}
        public string IdGrade { get; set; }
        public string IdUserLevelHead { get; set; }
        public string Data { get; set; }
        public IEnumerable<ClassAdvisor> ClassAdvisors { get; set; }
    }

    public class ClassAdvisor
    {
        public string IdClassroom { get; set; }
        public string IdUserClassAdvisor { get; set; }
        public string Data { get; set; }
    }
}
