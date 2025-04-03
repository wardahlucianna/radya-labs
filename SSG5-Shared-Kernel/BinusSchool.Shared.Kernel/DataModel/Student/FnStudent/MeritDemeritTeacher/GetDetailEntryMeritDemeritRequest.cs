using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailEntryMeritDemeritRequest
    {
        public string Id { get; set; }
        public MeritDemeritCategory Category { get; set; }
    }
}
