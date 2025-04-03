using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetUserActionNextByTeacherPositionResult
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdDepartment { get; set; }
        public string IdHomeroom { get; set; }
    }

    public class GetUserActionNextByTeacherPositionResult_PositionDetail
    {
        public string IdUser { get; set; }
        public string Name { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public OtherPositionByIdUserResult OtherPositions { get; set; }
    }

    public class GetUserActionNextByTeacherPositionResult_ScoreData
    {
        public string IdStudent { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdSubject { get; set; }
        public string IdDepartment { get; set; }
    }
}
