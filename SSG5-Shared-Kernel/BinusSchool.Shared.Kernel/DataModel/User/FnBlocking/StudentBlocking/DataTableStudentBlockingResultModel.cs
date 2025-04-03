using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class DataTableStudentBlockingResultModel
    {
        public int Draw { get; set; }

        public int RecordsTotal { get; set; }

        public int RecordsFiltered { get; set; }

        public object Data { get; set; }
    }


}
