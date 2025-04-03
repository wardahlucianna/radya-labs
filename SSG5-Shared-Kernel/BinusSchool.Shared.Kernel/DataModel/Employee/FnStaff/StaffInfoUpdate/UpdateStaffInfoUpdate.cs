using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Employee.FnStaff.StaffInfoUpdate
{
    public class UpdateStaffInfoUpdate
    {
        public string IdBinusian { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string OldFieldValue { get; set; }
        public string CurrentFieldValue { get; set; }
        public string Constraint3 { get; set; }
        public string Constraint3Value { get; set; }
        public int IdApprovalStatus { get; set; }
        public string Notes { get; set; }
    }
}
