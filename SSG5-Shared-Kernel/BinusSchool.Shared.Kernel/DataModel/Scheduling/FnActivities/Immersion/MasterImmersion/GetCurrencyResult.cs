using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class GetCurrencyResult : ItemValueVm
    {
        public string Currency { get; set; }
        public string Symbol { get; set; }
        public string CurrencyName { get; set; }
        public string IdCountry { get; set; }
        public string CountryName { get; set; }
    }
}
