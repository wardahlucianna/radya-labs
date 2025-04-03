using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterSearching
{
    public class GetListStudentByAySmtLvlGrdHrmResult : ItemValueVm
    {
        public NameValueVm Student { set; get; }
        public ItemValueVm Homeroom { set; get; }
    }
}
