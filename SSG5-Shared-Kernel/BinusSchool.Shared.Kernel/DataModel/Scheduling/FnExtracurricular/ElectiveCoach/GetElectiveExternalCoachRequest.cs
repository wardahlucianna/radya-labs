using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class GetElectiveExternalCoachRequest : CollectionRequest
    {
        public string IdSchool { set; get; }
        public GetElectiveExternalCoachRequest()
        {
        }
    }
}
