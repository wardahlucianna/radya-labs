using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.TransferStudentData
{
    public class TransferMasterCountryRequest
    {
        public IEnumerable<Country> CountryList { get; set; }     
    }
    public class Country{
         public string IdCountry { get; set; }
        public string CountryName { get; set; }
    }
}
