using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval
{
    public class GetEmailTextbookApprovalResult
    {
        public List<string> IdUserPic { get; set; }
        public string NameCreated { get; set; }
        public List<GetEmailTextbook> Textbooks { get; set; }
    }
}
