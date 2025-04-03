using System;
using System.Collections.Generic;
using System.Text;


namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class ExportExcelListPickupRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string? IdLevel { get; set; }
        public string? IdGrade { get; set; }
        public string? IdHomeroom { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
