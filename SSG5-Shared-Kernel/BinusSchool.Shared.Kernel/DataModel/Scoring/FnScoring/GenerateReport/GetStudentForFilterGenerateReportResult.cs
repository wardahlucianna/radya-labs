using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport
{
    public class GetStudentForFilterGenerateReportResult
    {
        public List<ItemValueVm> Student { get; set; }
    }

    public class GetStudentForFilterGenerateReportResult_StudentData
    {
        public string IdHomeroomStudent { set; get; }
        public string IdGrade { set; get; }
        public string GradeCode { set; get; }
        public string ClassroomCode { set; get; }
        public string IdStudent { set; get; }
        public string StudentFirstName { set; get; }
        public string StudentLastName { set; get; }
        //public int Semeseter { set; get; }
    }

    public class GetStudentForFilterGenerateReportResult_StudentStatus
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdPeriod { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
        public ItemValueVm StudentStatus { get; set; }
        public bool IsExitStudent { get; set; }
    }
}
