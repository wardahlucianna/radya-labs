using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor
{
    public class GetListCASStudentAdvisorResult
    {
        public string IdCASAdvisorStudent { get; set; }
        public GetListCASStudentAdvisorResult_Student Student { get; set; }
        public string Homeroom { get; set; }
        public GetListCASStudentAdvisorResult_Advisor Advisor { get; set; }
    }

    public class GetListCASStudentAdvisorResult_Student : ItemValueVm
    {
        public string IdHomeroomStudent { get; set; }
    }

    public class GetListCASStudentAdvisorResult_Advisor : ItemValueVm
    {
        public string IdCasAdvisor { get; set; }
    }
}
