using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GradePathwayForAscTimeTableResult
    {
        public string IdGrade { get; set; }
        public string IdGradePathway { get; set; }
        public string GradeCode { get; set; }
        public string GradeDescription { get; set; }
        public List<GradePathwayClassRoom> GradePathwayClassRooms { get; set; }
        public List<GradePathwayDetail> GradePathwayDetails { get; set; }
    }

    public class GradePathwayClassRoom
    {
        public string IdGradePathwayClassrom { get; set; }
        public string IdClassRoom { get; set; }
        public string ClassRoomCode { get; set; }
        public string ClassRoomDescription { get; set; }
        public string ClassRoomCombinationGrade { get; set; }
        public string IdSchool { get; set; }
    }

    public class GradePathwayDetail
    {
        public string IdGradePathwayDetail{ get; set; }
        public string IdPathway { get; set; }
        public string PathwayCode { get; set; }
        public string PathwayDescription { get; set; }
        public string ClassRoomCombinationGrade { get; set; }
        public string IdSchool { get; set; }
    }
}
