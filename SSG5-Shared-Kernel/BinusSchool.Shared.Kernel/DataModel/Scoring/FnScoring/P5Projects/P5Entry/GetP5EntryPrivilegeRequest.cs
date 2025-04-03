using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5Entry
{
    public class GetP5EntryPrivilegeRequest
    { 
        public string IdSchool { set; get; }
        public string IdUser { set; get; }
        public string IdAcademicYear { set; get; }
    }
}
