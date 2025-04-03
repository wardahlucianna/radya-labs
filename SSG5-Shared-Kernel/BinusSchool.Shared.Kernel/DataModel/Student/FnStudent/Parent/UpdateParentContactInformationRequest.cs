using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class UpdateParentContactInformationRequest
    {
        public string IdParent { get; set; }
        public string IdStudent { get; set; }
        public string IdParentRole { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string PersonalEmailAddress { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
