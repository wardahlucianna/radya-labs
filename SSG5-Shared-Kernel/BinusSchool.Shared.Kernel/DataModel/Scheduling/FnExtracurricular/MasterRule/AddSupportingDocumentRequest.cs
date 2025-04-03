using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule
{
    public class AddSupportingDocumentRequest
    {        
        public string Name { get; set; }
        public bool ShowToParent { get; set; }
        public bool ShowToStudent { get; set; }
        public bool Status { get; set; }
        public string FileName { get; set; }   
        public decimal FileSize { get; set; }
        public string Grades { get; set; }
    }
}
