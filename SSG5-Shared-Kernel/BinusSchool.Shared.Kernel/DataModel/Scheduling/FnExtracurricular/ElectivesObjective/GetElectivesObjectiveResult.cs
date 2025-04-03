using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesObjective
{
    public class GetElectivesObjectiveResult : CodeWithIdVm
    {
        public int OrderNumber { get; set; }        
        public string IdElectivesObjective { get; set; }
    }
}
