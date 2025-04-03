using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.BankAccountInformation
{
    public class UpdateBankAccountInformationRequest : AddBankAccountInformationRequest
    {
        public string BankAccountNameCurrentValue { set; get; }
        public string AccountNumberCurrentValue { set; get; }
        public string AccountNameCurrentValue { set; get; }
        public int IsParentUpdate { get; set; }
        //public string AccountNumberNewValue { set; get; }
        //public string AccountNameNewValue { set; get; }
        //public string BankAccountNameNewValue { set; get; }
        //public DateTime? RequestedDate { set; get; }
        //public DateTime? ApprovalDate { set; get; }
        //public DateTime? RejectDate { set; get; }
        //public int Status { set; get; }
        //public string Notes { set; get; }
    }
}
