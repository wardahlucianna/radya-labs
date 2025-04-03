using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDownloadStudentCertificationRequest
    {
        public string IdStudent { get; set; }
        public IEnumerable<string> IdAcadYears { get; set; }
    }
}