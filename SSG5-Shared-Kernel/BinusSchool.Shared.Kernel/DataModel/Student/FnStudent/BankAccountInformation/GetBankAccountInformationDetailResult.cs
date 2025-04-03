using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.BankAccountInformation
{
    public class GetBankAccountInformationDetailResult : GetBankAccountInformationResult
    {
        public AuditableResult Audit { get; set; }
    }
}
