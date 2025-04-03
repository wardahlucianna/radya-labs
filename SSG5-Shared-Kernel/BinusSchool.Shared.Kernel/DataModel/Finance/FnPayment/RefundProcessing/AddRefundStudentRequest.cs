namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class AddRefundStudentRequest
    {
        public string IdEventPayment { get; set; }
        public string IdRefundPayment { get; set; }
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
        public int TotalRefund { get; set; }
        public string AccountNumber { get; set; }
        public string IdBank { get; set; }
        public string AccountName { get; set; }
        public bool IsGenerate { get; set; }
        public bool IsUpdate { get; set; }
    }
}
