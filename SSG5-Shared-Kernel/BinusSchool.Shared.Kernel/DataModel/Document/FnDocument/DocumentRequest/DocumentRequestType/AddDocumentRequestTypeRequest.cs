using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class AddDocumentRequestTypeRequest
    {
        public string IdSchool { get; set; }
        public string DocumentName { get; set; }
        public string DocumentDescription { get; set; }
        public bool ActiveStatus { get; set; }
        public decimal Price { get; set; }
        public int? InvoicePaymentExpiredHours { get; set; }
        public int DefaultProcessDays { get; set; }
        public bool HardCopyAvailable { get; set; }
        public bool SoftCopyAvailable { get; set; }
        public string IdAcademicYearStart { get; set; }
        public string IdAcademicYearEnd { get; set; }
        public bool IsAcademicDocument { get; set; }
        public bool HasTermOptions { get; set; }
        public bool VisibleToParent { get; set; }
        public bool ParentNeedApproval { get; set; }
        public bool IsUsingNoOfPages { get; set; }
        public int? DefaultNoOfPages { get; set; }
        public bool IsUsingNoOfCopy { get; set; }
        public int? MaxNoOfCopy { get; set; }
        public List<string> CodeGrades { get; set; }
        public List<string> IdBinusianDefaultPICIndividuals { get; set; }
        public List<string> IdRoleDefaultPICGroups { get; set; }
        public List<AddDocumentRequestTypeRequest_AdditionalField> AdditionalFields { get; set; }
    }
    public class AddDocumentRequestTypeRequest_AdditionalField
    {
        public string QuestionDescription { get; set; }
        public int OrderNumber { get; set; }
        public bool IsRequired { get; set; }
        public string IdDocumentReqFieldType { get; set; }
        public string IdDocumentReqOptionCategory { get; set; }
    }
}
