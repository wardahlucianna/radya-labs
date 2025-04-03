using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Employee.MasterSearching
{
    public class GetMasterSearchingforStaffResult : CollectionRequest
    {
        public string BinusianID { get; set; }
        public string StaffName { get; set; }
        public string Email { get; set; }
        public string Initial { get; set; }
        public string Category { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string SchoolLocation { get; set; }
    }

}
