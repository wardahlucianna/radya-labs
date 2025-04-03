using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval
{
    public class GetTextbookPreparationApprovalResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string Subject { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string BookType { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public bool IsShowEdit { get; set; }
        public bool IsShowDelete { get; set; }
        public bool IsEnableApproval { get; set; }
        public bool IsEnableEdit { get; set; }
        public bool IsEnableDelete { get; set; }
    }
}
