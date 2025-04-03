using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway
{
    public class UpdateMapStudentPathwayRequest
    {
        public string IdGrade { get; set; }
        public IEnumerable<MapStudentAndPathway> Mappeds { get; set; }
    }

    public class MapStudentAndPathway
    {
        public string IdStudent { get; set; }
        public string IdPathway { get; set; }
        public string IdPathwayNextAcademicYear { get; set; }
    }
}
