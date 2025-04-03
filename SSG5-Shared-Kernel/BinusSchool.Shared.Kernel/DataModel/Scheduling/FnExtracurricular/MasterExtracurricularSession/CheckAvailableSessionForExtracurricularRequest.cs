using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricularSession
{
    public class CheckAvailableSessionForExtracurricularRequest
    {
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public List<ItemValueVm> Day { get; set; }
    }
}
