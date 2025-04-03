using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege;

namespace BinusSchool.Data.Model.Util.FnConverter.ServiceAsActionToPdf
{
    public class ConvertServiceAsActionToPdfRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdUser { get; set; }
        public bool IsIncludeComment { get; set; }
        public bool IsStudent { get; set; }
        public bool IsAdvisor { get; set; }
    }
}
