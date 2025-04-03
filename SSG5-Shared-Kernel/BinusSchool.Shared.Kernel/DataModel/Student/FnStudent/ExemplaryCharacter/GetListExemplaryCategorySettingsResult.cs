using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class GetListExemplaryCategorySettingsResult : ItemValueVm
    {
        public string IdExemplaryCategory { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public bool CurrentStatus { get; set; }
    }
}
