using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval
{
    public class TextbookPreparationApprovalRequest
    {
        public List<string> Ids { get; set; }
        public string IdUser { get; set; }
        public string Note { get; set; }
        public bool IsApproved { get; set; }    
    }
}
