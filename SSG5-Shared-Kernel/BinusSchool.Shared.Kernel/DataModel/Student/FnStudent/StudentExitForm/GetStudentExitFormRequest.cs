using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitForm
{
    public class GetStudentExitFormRequest : CollectionSchoolRequest
    {
        public string IdUser { get; set; }
    }
}
