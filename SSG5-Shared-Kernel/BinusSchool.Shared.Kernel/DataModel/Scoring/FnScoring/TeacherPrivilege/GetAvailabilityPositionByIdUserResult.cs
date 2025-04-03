using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetAvailabilityPositionByIdUserResult
    {
        public IReadOnlyList<ItemValueVm> ClassAdvisors { get; set; }
        public IReadOnlyList<ItemValueVm> SubjectTeachers { get; set; }
        public IReadOnlyList<OtherPositionByIdUserResult> OtherPositions { get; set; }
    }

    public class OtherPositionByIdUserResult : CodeWithIdVm
    {
        public string Data { get; set; }
    }
}
