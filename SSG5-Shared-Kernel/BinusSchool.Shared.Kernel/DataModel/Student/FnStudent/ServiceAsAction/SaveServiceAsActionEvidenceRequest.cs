using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction
{
    public class SaveServiceAsActionEvidenceRequest
    {
        public string IdServiceAsActionForm { get; set; }
        public string IdServiceAsActionEvidence { get; set; }
        public string EvidenceType { get; set; }
        public List<string> IdLoMappings { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public List<string> FIGM { get; set; }
    }
}
