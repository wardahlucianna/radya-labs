using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Information;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class UpdateInternationalStudentFormalitiesRequest
    {
        public string IdStudent { get; set; }
        public string KITASNumber { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public string NSIBNumber { get; set; }
        public DateTime? NSIBExpDate { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
