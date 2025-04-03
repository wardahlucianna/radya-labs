namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetEventParentAndStudentRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdEventType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
