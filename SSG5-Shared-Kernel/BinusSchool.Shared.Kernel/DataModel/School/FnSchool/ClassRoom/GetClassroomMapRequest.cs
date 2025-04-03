using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoom
{
    public class GetClassroomMapRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
        public string IdPathway { get; set; }
    }
}
