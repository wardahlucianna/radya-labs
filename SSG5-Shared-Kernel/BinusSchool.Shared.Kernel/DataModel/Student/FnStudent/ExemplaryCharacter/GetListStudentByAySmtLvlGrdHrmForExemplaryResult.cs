using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class GetListStudentByAySmtLvlGrdHrmForExemplaryResult : ItemValueVm
    {
        public NameValueVm Student { set; get; }
        public ItemValueVm Homeroom { set; get; }
        public bool IsBlocked { get; set; }
    }
}
