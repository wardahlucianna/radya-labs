using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class SaveServiceAsActionStatusRequest
    {
        public string IdGrade { get; set; }
        public string IdServiceAsActionForm { get; set; }
        public string IdServiceAsActionStatus { get; set; }
        public string RevisionNote { get; set; }
    }
}
