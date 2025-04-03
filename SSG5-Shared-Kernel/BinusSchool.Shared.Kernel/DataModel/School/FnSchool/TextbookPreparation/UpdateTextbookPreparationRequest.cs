using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparation
{
    public class UpdateTextbookPreparationRequest
    {
        public string Id { get; set; }
        public string IdUser { get; set; }
        public string IdSubjectGroup { get; set; }
        public string IdSubject { get; set; }
        public string IdStreaming { get; set; }
        public TextbookPreparationBookType BookType { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publish { get; set; }
        public decimal Weight { get; set; }
        public string NoSku { get; set; }
        public bool IsRegion { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsContinuity { get; set; }
        public bool IsAvailableStatus { get; set; }
        public string Supplier { get; set; }
        public string Location { get; set; }
        public string LastModif { get; set; }
        public string Vendor { get; set; }
        public int OriginalPrice { get; set; }
        public int PriceAfterDiscount { get; set; }
        public string Note { get; set; }
        public bool IsDraft { get; set; }
        public bool IsEditFromApprovalForm { get; set; }
        public int MinQty { get; set; }
        public int MaxQty { get; set; }
        public List<AddTextbookAttachment> Attachments { get; set; }
    }
}
