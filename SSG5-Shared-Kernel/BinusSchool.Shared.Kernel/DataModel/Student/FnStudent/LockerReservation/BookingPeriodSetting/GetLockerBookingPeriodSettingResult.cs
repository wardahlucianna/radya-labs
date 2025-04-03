using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class GetLockerBookingPeriodSettingResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public List<GetLockerBookingPeriodSettingResult_Grade> Grades { get; set; }
    }

    public class GetLockerBookingPeriodSettingResult_Grade
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int OrderNumber { get; set; }
    }
}
