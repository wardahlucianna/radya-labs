using System.Collections.Generic;
using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetTeacherPrivilegeResult
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public List<ItemValueVm> Roles { get; set; }
        public List<DetailNonteacingLoadAcademic> NonTeachingAssignmentAcademic { get; set; }
        public List<GroupNonteacingLoadNonAcademic> GroupNonteacingLoadNonAcademic { get; set; }
        public List<GetLevelList> LevelList { get; set; }
    }

    public class GroupNonteacingLoadNonAcademic
    {
        public string PositionName { get; set; }
        public string PositionShortName { get; set; }
        public List<HeadOfDepartmentData> HeadOfDepartmentData { get; set; }
        public List<ClassAdvisorData> ClassAdvisorData { get; set; }
        public List<LevelHeadData> LevelHeadData { get; set; }
        public List<SubjectHeadData> SubjectHeadData { get; set; }
        public List<SubjectHeadAssitantData> SubjectHeadAssitantData { get; set; }
    }

    public class GetLevelList
    {
        public ItemValueVm Level { get; set; }
        public List<GetGradeList> GradeList { get; set; }
    }

    public class GetGradeList
    {
        public ItemValueVm Grade { get; set; }
        public List<GetDepartmentList> DepartmentList { get; set; }
        //public List<GetHomeroomList> HomeroomList { get; set; }
        public List<GetSemesterList> SemesterList { get; set; }
    }

    public class GetSemesterList
    {
        public int Semester { get; set; }
        public List<GetHomeroomList> HomeroomList { get; set; }
    }

    public class GetDepartmentList
    {
        public ItemValueVm Department { get; set; }
        public List<ItemValueVm> SubjectList { get; set; }
    }

    public class GetHomeroomList
    {
        public ItemValueVm Homeroom { get; set; }
        public List<GetSubjectTypeList> SubjectTypeList { get; set; }
    }

    public class GetSubjectTypeList
    {
        public ItemValueVm SubjectType { get; set; }
        public List<GetSubjectList> SubjectList { get; set; }
        //public List<ItemValueVm> LessonList { get; set; }
    }

    public class GetSubjectList
    {
        public ItemValueVm Subject { get; set; }
        public List<ItemValueVm> LessonList { get; set; }
    }

    public class TeacherClaimsRole
     {
         public string Id { get; set; }
         public string Name { get; set; }
         public IEnumerable<ClaimsPermission> Permissions { get; set; }
     }

     public class ClaimsPermission
     {
         public string Id { get; set; }
         public string Name { get; set; }
     }

     public class DetailNonteacingLoadAcademic
     {
         public string Data { get; set; }
         public string PositionName { get; set; }
         public string PositionShortName { get; set; }
     }

     public class DetailNonteacingLoadNonAcademic
     {
         public string Data { get; set; }
         public string PositionName { get; set; }
         public string PositionShortName { get; set; }
         public HeadOfDepartmentData HeadOfDepartmentData { get; set; }
         public ClassAdvisorData ClassAdvisorData { get; set; }
         public LevelHeadData LevelHeadData { get; set; }
         public SubjectHeadData SubjectHeadData { get; set; }
         public SubjectHeadAssitantData SubjectHeadAssitantData { get; set; }
    }

    public class HeadOfDepartmentData
    {
        public ItemValueVm Departement { get; set; }
    }
    public class ClassAdvisorData
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Streaming { get; set; }
        public ItemValueVm Classroom { get; set; }
    }

    public class LevelHeadData
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
    }

    public class SubjectHeadData
    {
        public ItemValueVm Departement { get; set; }
        public ItemValueVm Subject { get; set; }
    }
    public class SubjectHeadAssitantData
    {
        public ItemValueVm Departement { get; set; }
        public ItemValueVm Subject { get; set; }
    }
}
