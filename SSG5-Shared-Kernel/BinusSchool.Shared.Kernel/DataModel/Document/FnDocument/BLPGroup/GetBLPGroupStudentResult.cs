using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPGroup
{
    public class GetBLPGroupStudentResult
    {
        public string? IdBLPGroupStudent { get; set; }
        public bool IsOpenPeriod { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm BLPStatus { get; set; }
        public ItemValueVm BLPGroup { get; set; }
        public FinalStatusDescription FinalStatus { get; set; }
        public DateTime? HardCopySubmissionDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }

    public class FinalStatusDescription
    {
        public BLPFinalStatus FinalStatusEnum { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
    }
}
