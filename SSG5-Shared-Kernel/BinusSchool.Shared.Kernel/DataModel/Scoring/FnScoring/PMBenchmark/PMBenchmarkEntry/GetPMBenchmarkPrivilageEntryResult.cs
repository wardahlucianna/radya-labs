using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PMBenchmark.PMBenchmarkEntry
{
    public class GetPMBenchmarkPrivilageEntryResult
    {
        public string IdAcademicYear { set; get; }
        public string AcademicYearName { set; get; }
        public string IdGrade { set; get; }
        public string GradeName { set; get; }
        public string IdTerm { set; get; }
        public string TermName { set; get; }
        public string IdAssessmentType { set; get; }
        public string AssessmentTypeName { set; get; }
        public string IdClass { set; get; }
        public string ClassName { set; get; }
    }
}
