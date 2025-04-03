using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesObjective
{
    public class AddElectivesObjectiveRequest
    {
        public string IdExtracurricular { get; set; }
        public List<ElectivesObjectiveRequestVm> ElectivesObjectives { get; set; }
    }
    public class ElectivesObjectiveRequestVm
    {
        public int OrderNumber { get; set; }
        public string IdElectivesObjective { get; set; }
        public string Description { get; set; }
    }
}
