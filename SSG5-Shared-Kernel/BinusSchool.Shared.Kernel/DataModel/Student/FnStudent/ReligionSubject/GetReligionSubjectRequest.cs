using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ReligionSubject
{
    public class GetReligionSubjectRequest : CollectionRequest
    {
        public string IdReligionSubject { get; set; }
    }
}