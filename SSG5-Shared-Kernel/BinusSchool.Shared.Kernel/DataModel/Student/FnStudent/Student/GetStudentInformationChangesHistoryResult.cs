using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentInformationChangesHistoryResult
    {
        public string DatabaseFieldName { get; set; }
        public string Constraint1Value { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public DateTime? ActionDate { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public ItemValueVm ApprovalStatus { get; set; }
        public string Notes { get; set; }
        public string ProposedBy { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
