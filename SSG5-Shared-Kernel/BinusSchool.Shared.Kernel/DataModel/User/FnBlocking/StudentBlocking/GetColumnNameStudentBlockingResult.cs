using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class GetColumnNameStudentBlockingResult
    {
        public ICollection<string> Columns1 { get; set; }
        public ICollection<string> Columns2 { get; set; }
    }

    public class GetColumnsNameQueryResult
    {
        public string BlockingCategory { get; set; }
        public string BlockingType { get; set; }
    }
}
