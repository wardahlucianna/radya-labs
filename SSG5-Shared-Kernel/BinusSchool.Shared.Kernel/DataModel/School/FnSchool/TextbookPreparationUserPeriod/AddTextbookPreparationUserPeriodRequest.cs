using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod
{
    public class AddTextbookPreparationUserPeriodRequest
    {
        public string IdAcademicYear { get; set; }
        public string GroupName { get; set; }
        public TextBookPreparationUserPeriodAssignAs AssignAs { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<TextBookPreparationUser> UserStaff { get; set; }
    }

    public class TextBookPreparationUser
    {
        public string IdRole { get; set; }
        public string IdPosition { get; set; }
        public string IdUser { get; set; }

    }
}
