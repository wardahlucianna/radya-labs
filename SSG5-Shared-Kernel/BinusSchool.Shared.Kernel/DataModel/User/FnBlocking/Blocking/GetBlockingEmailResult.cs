using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.Blocking
{
    public class GetBlockingEmailResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string BlockingCategoryName { get; set; }
        public string BlockingData { get; set; }
        public string SchoolName { get; set; }
        public List<DataParent> DataParents { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
    }
}
