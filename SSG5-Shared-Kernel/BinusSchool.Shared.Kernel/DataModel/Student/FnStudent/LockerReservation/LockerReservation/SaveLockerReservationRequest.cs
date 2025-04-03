using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation
{
    public class SaveLockerReservationRequest
    {
        public string IdLocker { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdReserver { get; set; }
        public bool IsAgree { get; set; }
        public string Notes { get; set; }
    }
}
