using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PrivilegeEntryScore
{
    public class GetPrivilegeEntryScoreByPositionResult
    {
        public string IdLevel  { set; get; }
        public string LevelName  { set; get; }
        public string IdGrade  { set; get; }
        public string GradeName  { set; get; }
        public string Semester  { set; get; }
        public string IdTerm  { set; get; }
        public string TermName  { set; get; }
        public string IdPathway  { set; get; }
        public string PathwayName  { set; get; }
        public string IdSubjectType  { set; get; }
        public string SubjectTypeName  { set; get; }
        public bool IsSubjectCLA { set; get; }
        public string IdSubject  { set; get; }
        public string SubjectName  { set; get; }
        public string IdSubjectLevel  { set; get; }
        public string SubjectLevelName  { set; get; }
        public string IdLesson  { set; get; }
        public string LessonName  { set; get; }
    }

    public class ClassAdvisorData
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Streaming { get; set; }
        public ItemValueVm Classroom { get; set; }
    }

}
