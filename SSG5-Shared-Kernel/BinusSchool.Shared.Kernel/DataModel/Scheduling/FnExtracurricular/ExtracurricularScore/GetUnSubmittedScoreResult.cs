using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetUnSubmittedScoreResult
    {
        public string Supervisior { get; set; }
        public ItemValueVm Extracurricular { get; set; }
        public List<ItemValueVm> Students { get; set; }
        public int Total { get; set; }
        public int Semester { get; set; }
    }
}
