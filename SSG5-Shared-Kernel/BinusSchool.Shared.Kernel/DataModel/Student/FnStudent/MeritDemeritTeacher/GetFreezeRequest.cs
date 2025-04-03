using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemerit
{
    public class GetFreezeRequest : CollectionSchoolRequest
    {
        public string IdAcademiYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public string IdHomeroom { get; set; }
        public bool? IsFreeze { get; set; }
    }
}
