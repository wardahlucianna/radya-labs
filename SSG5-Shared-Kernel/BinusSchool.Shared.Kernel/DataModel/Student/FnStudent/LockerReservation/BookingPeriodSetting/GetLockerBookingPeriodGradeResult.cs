using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class GetLockerBookingPeriodGradeResult
    {
        public List<GetLockerBookingPeriodGradeResult_Grade> Grades { get; set; }
        public string IdLevel { get; set; }
        public string LevelCode { get; set; }
        public string LevelDescription { get; set; }
    }

    public class GetLockerBookingPeriodGradeResult_Grade
    {
        public int OrderNumber { get; set; }
        public string Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool HasBookingPeriod { get; set; }
    }
}
