using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class GetAllLockerAllocationResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Building { get; set; }
        public ItemValueVm Floor { get; set; }
        public List<GetAllLockerAllocationResult_Grade> Grades { get; set; }
        public int TotalLocker { get; set; }
    }

    public class GetAllLockerAllocationResult_Grade
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int OrderNumber { get; set; }
    }
}
