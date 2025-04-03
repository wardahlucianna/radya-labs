using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class GetDetailStudentExtracurricularResult
    {
        public int MaxEffective { get; set; }
        public List<GetDetailStudentExtracurricularResult_Extracurricular> StudentExtracurricularList { get; set; }
    }

    public class GetDetailStudentExtracurricularResult_Extracurricular
    {
        public NameValueVm Extracurricular { get; set; }
        public NameValueVm Supervisor { get; set; }
        public NameValueVm Coach { get; set; }
        //public int Priority { get; set; }
        public bool IsPrimary { get; set; }
        public bool EnablePrimary { get; set; }
    }
}
