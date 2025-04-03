using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod
{
    public class DetailTextbookPreparationUserPeriodResult
    {
        public string Id { get; set; }
        public NameValueVm AcademicYear { get; set; }
        public string GroupName { get; set; }
        public string AssignAs { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public List<TextbookPreparationUser> Users { get; set; }
    }

    public class TextbookPreparationUser
    {
        public string IdBinusian { get; set; }
        public string StaffName { get; set; }
        public string IdRole { get; set; }
        public string Role { get; set; }
        public string IdPosition { get; set; }
        public string Position { get; set; }
    }
}
