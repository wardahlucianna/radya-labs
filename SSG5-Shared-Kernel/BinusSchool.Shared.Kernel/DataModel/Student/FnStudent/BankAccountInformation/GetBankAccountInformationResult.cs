using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.BankAccountInformation
{
    public class GetBankAccountInformationResult : CodeWithIdVm
    {
        public string IdBank { get; set; }
        public string IdStudent { get; set; }
        public string AccountNumberCurrentValue { set; get; }
        public string AccountNameCurrentValue { set; get; }
        public string BankAccountNameCurrentValue { set; get; }
        public string AccountNumberNewValue { set; get; }
        public string AccountNameNewValue { set; get; }
        public string BankAccountNameNewValue { set; get; }
        public DateTime? RequestedDate { set; get; }
        public DateTime? ApprovalDate { set; get; }
        public DateTime? RejectDate { set; get; }
        public int Status { set; get; }
        public string Notes { set; get; }
    }
}
