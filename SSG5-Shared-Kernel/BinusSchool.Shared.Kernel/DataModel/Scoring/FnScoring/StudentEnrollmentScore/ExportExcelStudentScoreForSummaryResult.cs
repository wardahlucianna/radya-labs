using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class ExportExcelStudentScoreForSummaryResult
    {
        public ItemValueVm Student { set; get; }
        public ItemValueVm Lesson { set; get; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
        public ItemValueVm Counter { set; get; }
        public ItemValueVm Teacher { set; get; }
        public string? Score { set; get; }
        public string? Category { set; get; }
        public ItemValueVm Level { set; get; }
        public CodeWithIdVm Grade { set; get; }
        public ItemValueVm Homeroom { set; get; }
        public ItemValueVm Subject { set; get; }
        public ItemValueVm Departement { set; get; }
        public ItemValueVm Term { get; set; }
    }
}
