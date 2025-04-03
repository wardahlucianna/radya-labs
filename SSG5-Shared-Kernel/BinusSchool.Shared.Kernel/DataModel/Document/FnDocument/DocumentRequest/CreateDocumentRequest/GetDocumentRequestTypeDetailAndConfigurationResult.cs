using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetDocumentRequestTypeDetailAndConfigurationResult
    {
        public NameValueVm DocumentRequest { get; set; }
        public string DocumentTypeDescription { get; set; }
        public bool DocumentHasTerm { get; set; }
        public decimal Price { get; set; }
        public bool HardCopyAvailable { get; set; }
        public bool SoftCopyAvailable { get; set; }
        public bool IsUsingNoOfPages { get; set; }
        public int? DefaultNoOfPages { get; set; }
        public bool IsUsingNoOfCopy { get; set; }
        public int? MaxNoOfCopy { get; set; }
        public int DefaultNoOfProcessDay { get; set; }
        public List<ItemValueVm> PICList { get; set; }
        public List<GetDocumentRequestTypeDetailAndConfigurationResult_FormField> AdditionalFieldsList { get; set; }
    }

    public class GetDocumentRequestTypeDetailAndConfigurationResult_FormField
    {
        public string IdDocumentReqFormField { get; set; }
        public ItemValueVm FieldType { get; set; }
        public bool HasOption { get; set; }
        public string QuestionDescription { get; set; }
        public int OrderNumber { get; set; }
        public bool IsRequired { get; set; }
        public List<GetDocumentRequestTypeDetailAndConfigurationResult_Option> Options { get; set; }
    }

    public class GetDocumentRequestTypeDetailAndConfigurationResult_Option
    {
        public string IdDocumentReqOption { get; set; }
        public string OptionDescription { get; set; }
    }
}
