using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class GetListStudentForRefundRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string? IdReligion { get; set; }
    }
}
