using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Bank
{
    public class GetBankRequest : CollectionRequest
    {
        public string IdBank { get; set; }
    }
}
