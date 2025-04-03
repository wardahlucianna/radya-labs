using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment
{
    public class TeacherAssignmentGetListResult : IItemValueVm
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string TeacherName { get; set; }
        public string Acadyear { get; set; }
        public string IdAcadyear { get; set; }
        public int NonTeachingLoad {get;set;}
        public int TotalLoad { get; set; }
        public string Status { get; set; }
        public IEnumerable<TeachingAssignmentVm> TeacingAssignment { get; set; }
        public IEnumerable<NonTeachingAssignmentAcademicVm> NonTeachingAssignmentAcademic { get; set; }
        public IEnumerable<NonTeachingAssignmentNonAcademicVm> NonTeachingAssignmentNonAcademic { get; set; }
        public IEnumerable<string> IdTimetable { get; set; }

        public bool IsAboveStandard {get;set;}
        public bool IsAssigned {get;set;}

        public int assignedCount {get;set;}
        public int unassignedCount {get;set;}
        public int aboveStandardCount {get;set;}
        public int totalTeacher {get;set;}
    }

    public class TeachingAssignmentVm
    {
        public string Id { get; set; }
        public CodeWithIdVm AcademicYear {get;set;}
        public string Subject { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm ClassAndSession { get; set; }
        public CodeWithIdVm Level {get;set;}
        public int Load {get;set;}
        public string IdUser {get;set;}
    }

    public class NonTeachingAssignmentAcademicVm
    {
        public string IdSchoolNonTeachingLoad { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }
    }

    public class NonTeachingAssignmentNonAcademicVm
    {
        public string IdSchoolNonTeachingLoad { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }
    }
}
