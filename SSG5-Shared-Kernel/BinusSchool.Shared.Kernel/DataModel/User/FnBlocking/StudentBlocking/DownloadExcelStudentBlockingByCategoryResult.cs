using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class DownloadExcelStudentBlockingByCategoryResult
    {
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string HomeRoom { get; set; }

        public List<DetailDownloadExcelStudentBlockingByCategoryResult> Details { get; set; }
    }

    public struct DetailDownloadExcelStudentBlockingByCategoryResult
    {
        public bool IsBlocked { get; set; }
        public string ColumnName { get; set; }
    }

    public struct StudentBlockingByCategoryResult
    {
        public string BlockingCategory { get; set; }
        public string BlockingType { get; set; }
        public string BlockingTypeId { get; set; }
    }
}
