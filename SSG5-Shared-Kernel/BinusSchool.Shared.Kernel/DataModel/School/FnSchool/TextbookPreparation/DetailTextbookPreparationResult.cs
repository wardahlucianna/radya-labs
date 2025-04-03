using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparation
{
    public class DetailTextbookPreparationResult
    {
        public string Id { get; set; }
        public NameValueVm UserCreate { get; set; }
        public NameValueVm AcademicYear { get; set; }
        public NameValueVm Level { get; set; }
        public NameValueVm Grade { get; set; }
        public NameValueVm SubjectGroup { get; set; }
        public NameValueVm Subject { get; set; }
        public NameValueVm Streaming { get; set; }
        public string BookType { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public decimal Weight { get; set; }
        public string NoSKU { get; set; }
        public bool IsRegion { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsCountinuity { get; set; }
        public bool IsAvailableStatus { get; set; }
        public string Supplier { get; set; }
        public string Location { get; set; }
        public string LastModif { get; set; }
        public string Vendor { get; set; }
        public int OriginalPrice { get; set; }
        public int PriceAfterDiscount { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public string NoteApproval { get; set; }
        public string LastApprovalName { get; set; }
        public DateTime LastUpdate { get; set; }
        public int MaxQty { get; set; }
        public int MinQty { get; set; }
        public List<DetailTextbookAttachment> Attachments { get; set; }
    }
    public class DetailTextbookAttachment
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string FileNameOriginal { get; set; }
    }
}
