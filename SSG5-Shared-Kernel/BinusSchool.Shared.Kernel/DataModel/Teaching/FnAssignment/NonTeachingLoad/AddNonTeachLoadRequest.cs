using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class AddNonTeachLoadRequest
    {
        public AddNonTeachLoadRequest()
        {
            Hierarchies = new List<string>();
        }
        public string IdAcadyear { get; set; }
        public string IdPosition { get; set; }
        public AcademicType Category { get; set; }
        public int Load { get; set; }
        public List<string> Hierarchies { get; set; }
    }
}
