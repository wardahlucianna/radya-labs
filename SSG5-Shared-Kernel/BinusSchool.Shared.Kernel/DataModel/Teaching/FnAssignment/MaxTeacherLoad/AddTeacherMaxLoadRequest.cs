using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.MaxTeacherLoad
{
    public class AddTeacherMaxLoadRequest
    {
        public string IdAcadyear { get; set; }
        public int MaxLoad { get; set; }
    }
}
