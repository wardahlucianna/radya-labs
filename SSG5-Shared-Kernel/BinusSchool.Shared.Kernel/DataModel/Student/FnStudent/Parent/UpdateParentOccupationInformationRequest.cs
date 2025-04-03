using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class UpdateParentOccupationInformationRequest
    {
        public string IdParent { get; set; }
        public string IdStudent { get; set; }
        public string IdParentRole { get; set; }
        public string IdOccupationType { get; set; }
        public string IdOccupationTypeDesc { get; set; }
        public int IdParentSalaryGroup { get; set; }
        public string IdParentSalaryGroupDesc { get; set; }
        public string OccupationPosition { get; set; }
        public string CompanyName { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
