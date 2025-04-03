using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparation
{
    public class DownloadTextbookPreparationResult
    {
        public string IdAcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string SubjectGroup { get; set; }
        public string Subject { get; set; }
        public string Streaming { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public decimal Weight { get; set; }
        public string IsMandatory { get; set; }
        public int MinQty { get; set; }
        public int MaxQty { get; set; }
        public string Continuity { get; set; }
        public string AvailableStatus { get; set; }
        public string Supplier { get; set; }
        public string Location { get; set; }
        public string LastModif { get; set; }
        public string Vendor { get; set; }
        public int OriginalPrice { get; set; }
        public int PriceAfterDiscount { get; set; }
        public string Relagion { get; set; }
        public string NoSku { get; set; }
        public string BookType { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public string UrlImg { get; set; }
    }
}
