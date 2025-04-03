using System;

namespace BinusSchool.Common.Model.Information
{
    public class BirthInfoVm
    {
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        /*public ItemValueVm IdBirthCountry { get; set; }
        public ItemValueVm IdBirthStateProvince { get; set; }
        public ItemValueVm IdBirthCity { get; set; }*/
        public ItemValueVm IdNationality { get; set; }
        public ItemValueVm IdCountry { get; set;}

        public string IdBirthCountry { get; set; }
        public string IdBirthStateProvince { get; set; }
        public string IdBirthCity { get; set; }
        /*public string IdNationality { get; set; }
        public string IdCountry { get; set; }*/
    }
}
