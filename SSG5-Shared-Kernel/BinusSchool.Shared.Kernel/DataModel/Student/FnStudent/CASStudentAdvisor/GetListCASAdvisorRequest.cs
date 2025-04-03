using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor
{
    public class GetListCASAdvisorRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
    }
}
