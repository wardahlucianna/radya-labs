using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparation
{
    public class GetUploadTextbookPreparationResult
    {
        public List<GetUploadTextbookPreparation> Filed { get; set; }
        public List<GetUploadTextbookPreparationSuccess> Success { get; set; }
    }

    public class GetUploadTextbookPreparationSuccess
    {
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string Level { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public string IdSubjectGroup { get; set; }
        public string SubjectGroup { get; set; }
        public string IdSubject { get; set; }
        public string Subject { get; set; }
        public string IdStreaming { get; set; }
        public string Streaming { get; set; }
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public TextbookPreparationBookType IdBookType { get; set; }
        public string BookType { get; set; }
        public string Publish { get; set; }
        public decimal Weight { get; set; }
        public bool IsMandator { get; set; }
        public int MinQty { get; set; }
        public int MaxQty { get; set; }
        public bool IsContinuity { get; set; }
        public bool IsAvailableStatus { get; set; }
        public string Supplier { get; set; }
        public string Location { get; set; }
        public string LastModif { get; set; }
        public string Vendor { get; set; }
        public int OriginalPrice { get; set; }
        public int PriceAfterDiscount { get; set; }
        public bool IsRelagion { get; set; }
        public string NoSku { get; set; }
        public string Note { get; set; }
        public string UrlImage { get; set; }
        public List<AddTextbookAttachment> Attachments { get; set; }
    }
}
