using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class GetLOVHomeroomResult : ItemValueVm
    {
        public int Semesester { get; set; }
        public string Code { get ; set; }
    }
}
