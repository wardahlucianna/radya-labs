using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Achievement
{
    public class GetUserByPositionRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string CodePosition { get; set;}
    }
}
