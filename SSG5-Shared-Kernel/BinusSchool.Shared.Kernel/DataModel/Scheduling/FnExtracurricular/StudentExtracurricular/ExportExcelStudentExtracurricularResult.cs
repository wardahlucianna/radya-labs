using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class ExportExcelStudentExtracurricularResult
    {
        public NameValueVm Homeroom { get; set; }
        public NameValueVm Student { get; set; }
        public List<GetDetailStudentExtracurricularResult_Extracurricular> ExtracurricularList { get; set; }
        public List<NameValueVm> PrimaryExtracurricularList { get; set; }
    }
    public class ExportExcelStudentExtracurricularResult_ParamDesc
    {
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public int Semester { get; set; }
        public string Homeroom { get; set; }
    }
}
