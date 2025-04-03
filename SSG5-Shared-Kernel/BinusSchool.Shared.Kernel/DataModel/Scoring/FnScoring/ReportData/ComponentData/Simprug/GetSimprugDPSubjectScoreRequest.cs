using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Simprug
{
    public class GetSimprugDPSubjectScoreRequest
    {
        public string ReportScoreTemplate { get; set; }
        public string ReportScoreTable { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
        public StudentProgrammeEnum?  StudentProgramme { get; set; }
        public string GradeCode { get; set; }
    }
}
