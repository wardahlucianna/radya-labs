using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway
{
    public class GetMapStudentPathwayRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
        public bool? IncludeLastHomeroom { get; set; }
        public IEnumerable<string> ExceptIds { get; set; }
    }
}