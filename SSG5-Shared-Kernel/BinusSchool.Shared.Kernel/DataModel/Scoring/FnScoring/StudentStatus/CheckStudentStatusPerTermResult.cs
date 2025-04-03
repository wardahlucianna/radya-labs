using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentStatus
{
    public class CheckStudentStatusPerTermResult
    {
        public string IdPeriod { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
        public ItemValueVm StudentStatus { get; set; }
        public bool IsExitStudent { get; set; }
    }
}
