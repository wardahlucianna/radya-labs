using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class SaveLockerAllocationRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdBuilding { get; set; }
        public string IdFloor { get; set; }
        public List<string> Grades { get; set; }
        public int TotalLocker { get; set; }
    }
}
