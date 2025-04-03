using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;


namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemerit
{
    public class UpdateFreezeRequest
    {
        public string IdHomeroomStudent { get; set; }
        public bool Isfreeze { get; set; }
    }
}
