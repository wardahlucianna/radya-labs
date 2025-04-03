using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping
{
    public class GetMappingClassRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdPathway { get; set; }
    }
}
