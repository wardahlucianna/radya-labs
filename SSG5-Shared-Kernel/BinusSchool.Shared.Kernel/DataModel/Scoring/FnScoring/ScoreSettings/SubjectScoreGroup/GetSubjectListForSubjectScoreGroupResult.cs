using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectScoreGroup
{
    public class GetSubjectListForSubjectScoreGroupResult
    {
        public CodeWithIdVm Level { set; get; }
        public CodeWithIdVm Grade { set; get; }
        public CodeWithIdVm Subject { set; get; }
    }

    public class GetSubjectListForSubjectScoreGroupResult_SubjectList
    {
        public string IdAcademicYear { set; get; }
        public string AcademicYearCode { set; get; }
        public string AcademicYearDesc { set; get; }
        public int OrderNoAcademicYear { set; get; }
        public string IdLevel { set; get; }
        public string LevelCode { set; get; }
        public string LevelDesc { set; get; }
        public int OrderNoLevel { set; get; }
        public string IdGrade { set; get; }
        public string GradeCode { set; get; }
        public string GradeDesc { set; get; }
        public int OrderNoGrade { set; get; }
        public string IdSubjectGroup { set; get; }
        public string SubjectGroup { set; get; }
        public int OrderNoSubjectGroup { set; get; }
        public string IdSubject { set; get; }
        public string SubjectCode { set; get; }
        public string SubjectDesc { set; get; }
    }
}
