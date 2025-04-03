using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.TransferStudentData
{
    public class TransferMasterNationalityRequest
    {
        public IEnumerable<Nationality> NationalityList { get; set; }     
            
        public class Nationality{
            public string IdNationality { get; set; }
            public string NationalityName { get; set; }
        }
    }
}
