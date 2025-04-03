using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule
{
    public class UpdateSupportingDocumentRequest 
    {
        public bool ActionUpdateStatus { get; set; }
        public string IdExtracurricularSupportDoc { get; set; }       
        public string Name { get; set; }
        public bool ShowToParent { get; set; }
        public bool ShowToStudent { get; set; }
        public bool Status { get; set; }
        public string FileName { get; set; }   
        public decimal FileSize { get; set; }
        public string Grades { get; set; }
    }
}
