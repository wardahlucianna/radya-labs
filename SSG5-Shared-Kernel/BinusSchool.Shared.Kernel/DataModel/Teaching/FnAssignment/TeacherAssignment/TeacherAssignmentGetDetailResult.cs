using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment
{
    public class TeacherAssignmentGetDetailResult
    {
        public string Id { get; set; }
        public string IdSchoolUser { get; set; }
        public string TeacherName { get; set; }
        public string IdSchoolAcademicYears { get; set; }
        public string Academicyears { get; set; }
        public int TeachingLoad { get; set; }
        public int NonTeacingLoadAcademic { get; set; }
        public int NonTeacingLoadNonAcademic { get; set; }
        public int TotalLoad { get; set; }
        public int MaxLoadinSchool { get; set; }
        // public string Status { get; set; }
        public List<NonteacingLoadAcademic> NonTeachingAssignmentAcademic { get; set; }
        public List<NonteacingLoadNonAcademic> NonTeachingAssignmentNonAcademic { get; set; }

        public List<ListNonTeachingAcademic> ListNonTeachingAcademics { get; set; }
        public List<ListNonTeachingAcademic> ListNonTeachingNonAcademics { get; set; }
    }

    public class ListNonTeachingAcademic
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Data { get; set; }
        public int Load { get; set; }
        public AcademicType Category { get; set; }
        public bool? IsHaveMasterForNextAy { get; set; }

    }
}
