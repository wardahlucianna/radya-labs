using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class ImportExcelInvoiceStudentResult
    {
        public int TotalRowData { get; set; }
        public int TotalSuccessRowData { get; set; }
        public int TotalFailRowData { get; set; }
        public List<string> ErrorImportExcelList { get; set; }
        public List<GetInvoiceStudentResult> StudentResultList { get; set; }
    }

    public class ImportExcelInvoiceStudentResult_ReadExcel
    {
        public string IdStudent { get; set; }
        public decimal Amount { get; set; }
    }
}
