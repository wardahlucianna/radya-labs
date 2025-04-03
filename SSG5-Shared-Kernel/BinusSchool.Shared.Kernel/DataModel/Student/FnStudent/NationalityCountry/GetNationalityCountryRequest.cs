using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.NationalityCountry
{
    public class GetNationalityCountryRequest : CollectionRequest
    {       
        public string IdCountry { get; set; }
    }
}
