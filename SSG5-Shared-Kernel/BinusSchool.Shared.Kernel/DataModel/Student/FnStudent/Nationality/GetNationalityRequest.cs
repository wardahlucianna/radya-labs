using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Nationality
{
    public class GetNationalityRequest : CollectionRequest
    {
        public string IdNationality { get; set; }
    }
}