using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetDocumentRequestTypeListResult : ItemValueVm
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
        public ItemValueVm AcademicYearStart { get; set; }
        public ItemValueVm AcademicYearEnd { get; set; }
        public bool IsAcademicDocument { get; set; }
        public bool DocumentHasTerm { get; set; }
        public bool IsUsingNoOfPages { get; set; }
        public int? DefaultNoOfPages { get; set; }
        public bool IsUsingNoOfCopy { get; set; }
        public int? MaxNoOfCopy { get; set; }
        public bool VisibleToParent { get; set; }
        public bool ParentNeedApproval { get; set; }
        public bool IsUsingGradeMapping { get; set; }
        public List<GetDocumentRequestTypeListResult_GradeMapping> GradeMappingList { get; set; }
        public List<GetDocumentRequestTypeListResult_PICIndividual> PICIndividualList { get; set; }
        public List<GetDocumentRequestTypeListResult_PICGroup> PICGroupList { get; set; }
    }

    public class GetDocumentRequestTypeListResult_GradeMapping
    {
        public string GradeCode { get; set; }
        public string GradeDescription { get; set; }
    }

    public class GetDocumentRequestTypeListResult_PICIndividual
    {
        public NameValueVm Binusian { get; set; }
    }

    public class GetDocumentRequestTypeListResult_PICGroup
    {
        public ItemValueVm RoleGroup { get; set; }
    }
}
