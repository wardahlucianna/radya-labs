using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation
{
    public class GetStudentLockerDataResult
    {
        public string IdLocker { get; set; }
        public string LockerName { get; set; }
        public string IdLockerPosition { get; set; }
        public bool LockerPosition { get; set; }
        public string LockerPositionName { get; set; }
        public string IdFloor { get; set; }
        public string FloorName { get; set; }
        public string IdBuilding { get; set; }
        public string BuildingName { get; set; }
        public bool Status { get; set; }
        public string IdStudentLockerReservation { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdGrade { get; set; }
        public string GradeName { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
        public string IdReserver { get; set; }
        public bool IsAgree { get; set; }
        public string Notes { get; set; }
    }
}
