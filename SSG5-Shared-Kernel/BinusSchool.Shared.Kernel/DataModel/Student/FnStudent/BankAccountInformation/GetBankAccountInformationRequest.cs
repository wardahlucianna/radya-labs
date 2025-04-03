using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.BankAccountInformation
{
    public class GetBankAccountInformationRequest : CollectionRequest
    {
        public string IdBank { get; set; }
        public string IdStudent { get; set; }
    }
}
