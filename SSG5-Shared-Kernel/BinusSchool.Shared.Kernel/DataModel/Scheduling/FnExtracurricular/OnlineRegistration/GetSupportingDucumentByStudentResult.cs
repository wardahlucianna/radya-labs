using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetSupportingDucumentByStudentResult     
    {
        public string IdExtracurricularSupportDoc { get; set; }
        public string DocumentName { get; set; }
        public string DocumentLink { get; set; }
        public string FileName { get; set; }
        public ItemValueVm Grade { get; set; }      
        public bool Status { get; set; }
    }
}
