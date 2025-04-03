using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.LockerReservation.StudentBooking
{
    public class GetStudentBookingResult
    {
        public bool AvailableBooking { set; get; }
        public string Msg { set; get; }
        public string BookingPeriod { set; get; }  
        public StudentBooking_StudDataVm StudentData { set; get; }
        public List<StudentBooking_LockerLocationVm> LockerLocations { set; get; }
        public string PolicyMessage { set; get; }
    }
    public class StudentBooking_LockerLocationVm
    {
        public CodeWithIdVm LockerLocation { set; get; }
        public List<CodeWithIdVm> LockerPosition { set; get; }
    }
    
    public class StudentBooking_StudDataVm
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }

    }
}
