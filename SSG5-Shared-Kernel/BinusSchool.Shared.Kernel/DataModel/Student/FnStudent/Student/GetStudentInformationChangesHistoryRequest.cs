using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentInformationChangesHistoryRequest : CollectionRequest
    {
        public string IdStudent { get; set; }
        public List<string> IdStudentInfoUpdateList { get; set; }
    }
}
