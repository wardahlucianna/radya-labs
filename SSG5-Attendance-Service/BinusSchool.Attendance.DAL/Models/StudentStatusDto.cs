using System;

namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class StudentStatusDto
    {
        public string IdStudent { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDt { get; set; }
        public DateTime? EndDt { get; set; }
    }
}
