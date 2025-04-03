using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class UploadExcelContentDataStudentBlockingValidationResult
    {
        public ICollection<string> Columns1 { get; set; }
        public ICollection<string> Columns2 { get; set; }
    }

    public class UploadExcelColumnNameStudentBlockingValidationQueryResult
    {
        public string BlockingCategory { get; set; }
        public string BlockingType { get; set; }
    }

    public class UploadExcelContentDataStudentBlockingValidationQueryResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdBlockingCategory { get; set; }
        public string BlockingCategory { get; set; }
        public string IdBlockingType { get; set; }
        public string BlockingType { get; set; }
        public bool IsBlocking { get; set; }
        public bool CanEdit { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsStudentID { get; set; }
        public bool IsNameStudent { get; set; }
        public bool IsRealtionBLockingCategoryType { get; set; }
    }

}
