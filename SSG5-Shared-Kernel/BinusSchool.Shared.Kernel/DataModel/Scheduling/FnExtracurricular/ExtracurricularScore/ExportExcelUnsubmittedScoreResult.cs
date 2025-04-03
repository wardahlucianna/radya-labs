using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class ExportExcelUnsubmittedScoreResult
    {
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public NameValueVm Extracurricular { get; set; }
        public string ElectiveGrade { get; set; }
        public List<ExportExcelUnsubmittedScoreStudentScore> StudentScore { get; set; }
        public ExportExcelUnsubmittedScoreUnsubmittedScore UnsubmittedScore { get; set; }
    }

    public class ExportExcelUnsubmittedScoreStudentScore
    {
        public NameValueVm Student { get; set; }
        public string Class { get; set; }
        public string StudentGrade { get; set; }
    }

    public class ExportExcelUnsubmittedScoreUnsubmittedScore
    {
        public string SpvCoach { get; set; }
        public string TotalParticipant { get; set; }
        public string TotalEntry { get; set; }
    }
}
