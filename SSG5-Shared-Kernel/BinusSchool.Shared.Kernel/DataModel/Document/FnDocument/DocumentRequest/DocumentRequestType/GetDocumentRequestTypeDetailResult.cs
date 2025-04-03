using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetDocumentRequestTypeDetailResult
    {
        public string IdDocumentReqType { get; set; }
        public bool ActiveStatus { get; set; }
        public bool CanDelete { get; set; }
        public string IdSchool { get; set; }
        public string DocumentName { get; set; }
        public string DocumentDescription { get; set; }
        public decimal Price { get; set; }
        public int? InvoiceDueHoursPayment { get; set; }
        public int DefaultNoOfProcessDay { get; set; }
        public bool HardCopyAvailable { get; set; }
        public bool SoftCopyAvailable { get; set; }
        public CodeWithIdVm AcademicYearStart { get; set; }
        public CodeWithIdVm AcademicYearEnd { get; set; }
        public bool IsAcademicDocument { get; set; }
        public bool DocumentHasTerm { get; set; }
        public bool IsUsingNoOfPages { get; set; }
        public int? DefaultNoOfPages { get; set; }
        public bool IsUsingNoOfCopy { get; set; }
        public int? MaxNoOfCopy { get; set; }
        public bool VisibleToParent { get; set; }
        public bool ParentNeedApproval { get; set; }
        public bool IsUsingGradeMapping { get; set; }
        public List<string> CodeGrades { get; set; }
        public List<NameValueVm> PICIndividualList { get; set; }
        public List<ItemValueVm> PICGroupList { get; set; }
        public List<GetDocumentRequestTypeDetailResult_FormField> FormFieldList { get; set; }
    }

    public class GetDocumentRequestTypeDetailResult_FormField
    {
        public string IdDocumentReqFormField { get; set; }
        public ItemValueVm FieldType { get; set; }
        public bool HasOption { get; set; }
        public string QuestionDescription { get; set; }
        public int OrderNumber { get; set; }
        public bool IsRequired { get; set; }
        public GetDocumentRequestTypeDetailResult_OptionCategory OptionCategory { get; set; }
    }

    public class GetDocumentRequestTypeDetailResult_OptionCategory
    {
        public string IdDocumentReqOptionCategory { get; set; }
        public ItemValueVm FieldType { get; set; }
        public string CategoryDescription { get; set; }
        public bool IsDefaultImportData { get; set; }
    }
}
