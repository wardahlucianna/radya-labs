using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification
{
    public class GetListStudentCertificationResult : CodeWithIdVm
    {
        public string AcadYear { get; set; }
        public string IdLevel { get; set; }
        public string Level { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
        public string IdPeriod { get; set; }
        public string Period { get; set; }
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public List<string> ClassIds { get; set; }
        public string IdHomeroomStudent { get; set; }

    }
}
