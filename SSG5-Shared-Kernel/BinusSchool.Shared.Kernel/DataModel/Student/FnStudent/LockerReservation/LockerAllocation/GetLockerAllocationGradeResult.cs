using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class GetLockerAllocationGradeResult
    {
        public string IdLevel { get; set; }
        public string LevelCode { get; set; }
        public string LevelDesc { get; set; }
        public List<GetLockerAllocationGradeResult_Grade> Grades { get; set; }
    }

    public class GetLockerAllocationGradeResult_Grade
    {
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
        public string GradeDesc { get; set; }
        public int OrderNumber { get; set; }
        public bool HasAllocated { get; set; }
    }
}
