using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective
{
    public class GetAvailabilityPositionUserElectiveResult
    {
        public IReadOnlyList<ItemValueVm> ClassAdvisors { get; set; }
        public IReadOnlyList<ItemValueVm> SubjectTeachers { get; set; }
        public IReadOnlyList<OtherPositionUserElectiveResult> OtherPositions { get; set; }
    }

    public class OtherPositionUserElectiveResult : CodeWithIdVm
    {
        public string Data { get; set; }
    }
}
