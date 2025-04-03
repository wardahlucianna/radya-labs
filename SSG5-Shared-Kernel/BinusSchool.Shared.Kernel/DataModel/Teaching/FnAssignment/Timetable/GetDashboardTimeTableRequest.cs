namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class GetDashboardTimeTableRequest
    {
        //public string IdSchool {get;set;}
        public string IdAcademicyear {get;set;}
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubjetc { get; set; }
        public string IdClass { get; set; }
        public string IdDepartment { get; set; }
        public string IdStreaming { get; set; }
        public bool? Status { get; set; }
    }
}
