using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentStatus
{
    public class CheckStudentStatusPerSemesterResult
    {
        public int Semester { get; set; }
        public ItemValueVm StudentStatus { get; set; }
        public bool IsExitStudent { get; set; }
    }
}
