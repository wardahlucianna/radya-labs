using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class GetLockerAllocationGradeRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdBuilding { get; set; }
        public string IdFloor { get; set; }
    }
}
