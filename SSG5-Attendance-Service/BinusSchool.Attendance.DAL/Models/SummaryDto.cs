using System.Collections.Generic;

namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class SummaryDto
    {
        public string IdStudent { get; set; }
        public string IdPeriod { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdSchool { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public int Term { get; set; }
        public int Semester { get; set; }
        public List<Summary> Items { get; set; }
        public string JsonName { get; set; }
    }
}
