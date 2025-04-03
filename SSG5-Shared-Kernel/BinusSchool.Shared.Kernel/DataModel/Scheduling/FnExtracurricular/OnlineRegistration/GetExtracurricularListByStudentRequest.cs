using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetExtracurricularListByStudentRequest : CollectionRequest
    {
        //public string IdAcademicYear { get; set; }
        //public string IdGrade { get; set; }
        //public int Semester { get; set; }
        //public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
    }
}
