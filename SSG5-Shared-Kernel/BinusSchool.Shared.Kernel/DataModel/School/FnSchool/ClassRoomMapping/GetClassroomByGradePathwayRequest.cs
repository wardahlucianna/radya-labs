using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping
{
    public class GetClassroomByGradePathwayRequest : IdCollection
    {
        public string Search { get; set; }
    }
}
