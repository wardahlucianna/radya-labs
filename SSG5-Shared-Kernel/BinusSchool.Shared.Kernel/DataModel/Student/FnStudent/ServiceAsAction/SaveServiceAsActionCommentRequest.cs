using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class SaveServiceAsActionCommentRequest
    {
        public bool IsAdvisor { get; set; }
        public string IdUser { get; set; }
        public string IdServiceAsActionComment { get; set; }
        public string IdServiceAsActionEvidence { get; set; }
        public string Comment { get; set; }
    }
}
