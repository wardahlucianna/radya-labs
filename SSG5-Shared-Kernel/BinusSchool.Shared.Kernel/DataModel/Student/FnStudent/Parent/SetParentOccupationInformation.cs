using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class SetParentOccupationInformation
    {
        public ItemValueVm IdOccupationType { get; set; }
        public ItemValueVm IdParentSalaryGroup { get; set; }
        public string OccupationPosition { get; set; }
        public string CompanyName { get; set; }
    }
}
