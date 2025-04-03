using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate
{
    public class GetStudentInfoUpdateApprovalResult
    {
        public string Grade { get; set; }
        public string ClassRoom { get; set; }
        public List<string> ParentUpdatelist { get; set; }
        public int ParentUpdate { get; set; }
        public List<string> StaffUpdatelist { get; set; }
        public int StaffUpdate { get; set; }
    }
}
