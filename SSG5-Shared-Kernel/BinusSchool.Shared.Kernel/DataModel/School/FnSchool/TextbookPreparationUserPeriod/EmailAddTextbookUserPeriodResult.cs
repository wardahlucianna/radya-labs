using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod
{
    public class EmailAddTextbookUserPeriodResult
    {
        public string Id { get; set; }
        public string NamaAdministrator { get; set; }
        public TextBookPreparationUserPeriodAssignAs AssignAs { get; set; }
        public List<string> IdUserEntry { get; set; }
    }
}
