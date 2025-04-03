using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeachingLoad
{
    public class GetTeacherLoadResult : CodeWithIdVm
    {
        public string BinusianId { get; set; }
        public int TeachingLoad { get; set; }
        public int NonTeachingLoad { get; set; }
        public int TotalLoad { get; set; }

        public bool AssignCA { get; set; }
    }
}
