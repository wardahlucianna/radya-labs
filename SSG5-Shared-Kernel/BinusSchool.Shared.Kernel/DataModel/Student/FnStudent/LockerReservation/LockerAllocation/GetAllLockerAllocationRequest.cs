using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class GetAllLockerAllocationRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
