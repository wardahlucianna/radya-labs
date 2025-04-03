using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting
{
    public class GetDigitalPickupSettingRequest
    {
        public string IdAcademicYear { get; set; }
        public List<string>? IdLevel { get; set; }
        public string? IdStatus { get; set; }
    }
}
