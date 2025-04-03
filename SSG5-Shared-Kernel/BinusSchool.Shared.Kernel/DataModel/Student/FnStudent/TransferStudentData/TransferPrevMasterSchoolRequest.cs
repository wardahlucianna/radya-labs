using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.TransferStudentData
{
    public class TransferPrevMasterSchoolRequest
    {
        public IEnumerable<PrevMasterSchool> PrevMasterSchoolList { get; set; }
    }

    public class PrevMasterSchool
    {
        public string IdPrevMasterSchool { get; set; } 
        public string NPSN { get; set; } 
        public string TypeLevel { get; set; }     
        // public string SchoolTypeName { get; set; }  
        public string SchoolName { get; set; }  
        public string Address { get; set; }  
        public string Country { get; set; }  
        public string Province { get; set; }  
        public string Kota_kab { get; set; } 
        public string Website { get; set; }  

    }
    
}
