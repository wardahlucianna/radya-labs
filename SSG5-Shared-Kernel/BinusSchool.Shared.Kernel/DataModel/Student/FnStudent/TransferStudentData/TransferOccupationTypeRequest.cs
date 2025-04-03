using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.TransferStudentData
{
    public class TransferOccupationTypeRequest
    {      
        public IEnumerable<OccupationType> OccupationTypeList { get; set; }
    }


    public class OccupationType
    {

        public string IdOccupationType { get; set; }
        public string OccupationTypeName { get; set; }
        public string OccupationTypeNameEng { get; set; }
    }
}
