using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class ExportExcelListPickupResult
    {
        public string SchoolName { get; set; }
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string Level { get; set; } = "All";
        public string Grade { get; set; } = "All";
        public string Class { get; set; } = "All";
        public List<ExportExcelListPickup_PickupData> PickupData { get; set; }
    }

    public class ExportExcelListPickup_PickupData
    {
        public DateTime Date { get; set; }
        public string Homeroom { get; set; }
        public NameValueVm Student { get; set; }
        public DateTime QrScanTime { get; set; }
        public DateTime? PickupTime { get; set; }
    }
}
