using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification
{
    public class GetAcadYearByStudentResult : CodeWithIdVm
    {
        public IEnumerable<Grade> Grade { get; set; }

    }

    public class Grade
    {
        public string IdGrade { get; set; }
        public string DescriptionGrade { get; set; }
        public IEnumerable<AcademicYear> AcadYear { get; set; }
    }

    public class AcademicYear : CodeWithIdVm
    {
    }

    public class GetListStudentGrade
    {
        public string IdAcadyear { get; set; }
        public string AcadYear { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public string StudentName { get; set; }
        public string BinusianId { get; set; }

    }
}
