using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance
{
    public class GetUnsubmittedStudentsRequest : CollectionRequest
    {
        public DateTime Date { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
    }
}
