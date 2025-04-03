using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway
{
    public class CopyMapStudentPathwayRequest
    {
        public List<MapStudentPathway> MapStudentPathway { get; set; }
    }

    public class MapStudentPathway
    {
        public string IdStudentGradeNextAy { get; set; }
        public string IdStudentGradeCurrentAy { get; set; }
        public string IdPathwayNextAcademicYear { get; set; }
    }
}
