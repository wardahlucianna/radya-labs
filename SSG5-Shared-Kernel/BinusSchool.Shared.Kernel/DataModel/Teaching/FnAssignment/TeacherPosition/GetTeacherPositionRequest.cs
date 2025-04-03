using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition
{
    public class GetTeacherPositionRequest : CollectionSchoolRequest
    {
        public IEnumerable<string> PositionCode {get;set;}
    }
}
