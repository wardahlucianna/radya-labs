using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class GetListPickedUpRequest
    {
        public DateTime Date { get; set; }
        public string? IdLevel { get; set; }
        public string? IdGrade { get; set; }
        public string? IdHomeroom { get; set; }
        public string Status { get; set; }
        public string IdSchool { get; set; }
    }
}
