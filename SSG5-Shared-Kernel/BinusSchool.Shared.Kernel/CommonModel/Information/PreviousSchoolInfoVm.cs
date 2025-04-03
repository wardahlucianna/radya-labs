using System;

namespace BinusSchool.Common.Model.Information
{
    public class PreviousSchoolInfoVm
    {
        public ItemValueVm IdPreviousSchoolNew { get; set; }
        public string Grade { get; set; }
        public string YearAttended { get; set; }
        public string YearWithdrawn { get; set; }
        public Int16 IsHomeSchooling { get; set; }
    }
}
