using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class GetStudentBlockingCategoryTypeResult
    {
        public bool IsBlocked { get; set; }
        public List<GetStudentBlockingCategoryTypeResult_Detail> Details { get; set; }
    }

    public class GetStudentBlockingCategoryTypeResult_Detail
    {
        public ItemValueVm BlockingCategory { get; set; }
        public ItemValueVm BlockingType { get; set; }
    }
}
