using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class GetListExemplaryValueSettingsResult : ItemValueVm
    {
        public string IdExemplaryValue { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public int OrderNumber { get; set; }
        public bool CurrentStatus { get; set; }
    }
}
