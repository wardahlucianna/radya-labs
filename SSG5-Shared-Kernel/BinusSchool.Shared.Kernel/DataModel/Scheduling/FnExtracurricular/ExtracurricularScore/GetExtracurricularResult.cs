using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetExtracurricularResult
    {
        public List<ExtracurricularScoreVm> Extracurriculars { get; set; }
        public List<ItemValueVm> ScoreLegends { get; set; }
    }
    public class ExtracurricularScoreVm
    {
        public string IdExtracurricular { get; set; }
        public string ExtracurricularName { get; set; }
        public DateTime? ScoreStartDate { get; set; }
        public DateTime? ScoreEndDate { get; set; }
        public bool InPeriodEntry { get; set; }
        public string Supervisor { get; set; }
        public string Coach { get; set; }
    }

}
