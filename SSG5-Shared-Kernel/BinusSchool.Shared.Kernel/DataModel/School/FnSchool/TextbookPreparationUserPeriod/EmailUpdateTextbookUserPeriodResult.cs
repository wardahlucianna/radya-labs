using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod
{
    public class EmailUpdateTextbookUserPeriodResult
    {
        public string Id { get; set; }
        public string NamaAdministrator { get; set; }
        public string DateStartOld { get; set; }
        public string DateEndOld { get; set; }
        public string DateStartNew { get; set; }
        public string DateEndNew { get; set; }
        public TextBookPreparationUserPeriodAssignAs AssignAs { get; set; }
        public string AssignAsString { get; set; }
        public List<string> IdUserEntry { get; set; }
    }
}
