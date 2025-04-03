using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparation
{
    public class GetTextbookPreparationResult : CodeWithIdVm
    {
        public bool IsEnableButtonAdd { get; set; }
        public List<GetTextbookPreparation> TextbokPreparations { get; set; }
    }

    public class GetTextbookPreparation
    {
        public string Id { get; set; }
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
        public bool IsDisabledEdit { get; set; }
        public bool IsDisabledDelete { get; set; }
    }
}
