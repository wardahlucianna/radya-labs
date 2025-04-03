using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Employee.MasterSearching
{
    public class GetMasterSearchingDataforStaffRequest : CollectionRequest
    {
        public string UserId { get; set; }
        public int DesignationId { get; set; }
        public string BinusianID { get; set; }
        public string StaffName { get; set; }
        public string Email { get; set; }
        public string Initial { get; set; } 
        public string Position { get; set; }
        public string Department { get; set; }
        public string SchoolId { get; set; }
        public string SchoolName { get; set; }
    }
}
