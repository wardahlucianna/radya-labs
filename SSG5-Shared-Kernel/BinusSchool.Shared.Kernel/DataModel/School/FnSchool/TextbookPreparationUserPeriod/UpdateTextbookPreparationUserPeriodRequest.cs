using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod
{
    public class UpdateTextbookPreparationUserPeriodRequest
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<TextBookPreparationUser> UserStaff { get; set; }
    }
}
