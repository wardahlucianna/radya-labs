using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation
{
    public class GetLockerListResult
    {
        public List<GetLockerListResult_Detail> UpperLocker { get; set; }
        public List<GetLockerListResult_Detail> LowerLocker { get; set; }
    }

    public class GetLockerListResult_Detail
    {
        public string IdLocker { get; set; }
        public bool LockerPosition { get; set; }
        public string LockerPositionName { get; set; }
        public string LockerName { get; set; }
        public GetLockerListResult_LockerReservation LockerReservation { get; set; }
        public string FloorName { get; set; }
        public string BuildingName { get; set; }
        public string Status { get; set; }
    }

    public class GetLockerListResult_LockerReservation
    {
        public string IdStudentLockerReservation { get; set; }
        public NameValueVm Student { get; set; }
        public string Homeroom { get; set; }
    }
}
