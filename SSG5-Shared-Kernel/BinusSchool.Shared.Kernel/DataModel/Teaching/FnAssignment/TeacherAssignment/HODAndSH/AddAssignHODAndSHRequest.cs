using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH
{
    public class AddAssignHODAndSHRequest
    {
        public string IdSchool { get; set; }
        public string IdSchoolAcadYear { get; set; }
        public string IdSchoolDepartment { get; set; }
        public string IdSchoolUserHeadOfDepartment { get; set; }
        public string IdSchoolNonTeachingLoadDepartment { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }
        public List<SubjectHeadRequest> SubjectHeads { get; set; }


    }

    public class SubjectHeadRequest
    {
        public string Id { get; set; }
        public string IdSchoolUser { get; set; }
        public SubjectHeadAssitanceRequest SubjectHeadAssitance { get; set; }
        public string IdSchoolNonTeachingLoad { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }
    }

    public class SubjectHeadAssitanceRequest
    {
        public string Id { get; set; }
        public string IdSchoolUser { get; set; }
        public string IdSchoolNonTeachingLoad { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }
    }
}
