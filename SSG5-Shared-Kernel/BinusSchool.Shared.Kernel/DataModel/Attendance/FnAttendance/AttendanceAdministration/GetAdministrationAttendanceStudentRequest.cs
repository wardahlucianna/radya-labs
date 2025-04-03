using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class GetAdministrationAttendanceStudentRequest : CollectionSchoolRequest
    {
        public string IdGrade { get; set; }
        public string IdPathway { get; set; }
        public IEnumerable<string> IdHomeroom { get; set; }
        public IEnumerable<string> IdSubject { get; set; }
        public IEnumerable<string> IdStudent { get; set; }
    }
}
