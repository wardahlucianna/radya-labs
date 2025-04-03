using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class GetServiceAsActionDetailFormRequest
    {
        public string IdUser { get; set; }
        public string IdServiceAsActionForm { get; set; }
        public bool IsIncludeComment { get; set; }
        public bool? IsAdvisor { get; set; }
    }
}
