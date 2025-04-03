using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule
{
    public class GetSupportingDucumentRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public bool? Status { get; set; }
    }
}
