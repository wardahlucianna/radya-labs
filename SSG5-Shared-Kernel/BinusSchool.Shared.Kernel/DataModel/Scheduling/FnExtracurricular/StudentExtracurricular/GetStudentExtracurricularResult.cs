using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class GetStudentExtracurricularResult
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public NameValueVm Homeroom { get; set; }
        public NameValueVm Student { get; set; }
        public int TotalExtracurricular { get; set; }
        public List<NameValueVm> PrimaryExtracurricular { get; set; }
    }
}
