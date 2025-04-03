namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class GetExcelParamDescriptionResult
    {
        public string School { set; get; }
        public string AcademicYear { set; get; }
        public string Semester { set; get; }
        public string RefundName { set; get; }
        public string ExpectedRefundDate { set; get; }
        public string RefundDate { get; set; }
    }
}
