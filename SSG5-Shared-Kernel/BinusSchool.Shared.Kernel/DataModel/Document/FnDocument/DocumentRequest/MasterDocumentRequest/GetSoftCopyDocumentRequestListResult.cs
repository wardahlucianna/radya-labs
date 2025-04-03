using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetSoftCopyDocumentRequestListResult
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public string DocumentTypeName { get; set; }
        public ItemValueVm AcademicYearDocument { get; set; }
        public ItemValueVm GradeDocument { get; set; }
        public string HomeroomNameDocument { get; set; }
        public ItemValueVm PeriodDocument { get; set; }
        public bool NeedHardCopy { get; set; }
        public bool NeedSoftCopy { get; set; }
        public bool IsAttachmentShowToParent { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public NameValueVm LastUpdateBy { get; set; }
        public bool CanDeleteAttachment { get; set; }
        public bool CanEditCbShowToParent { get; set; }
    }
}
