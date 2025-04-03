using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Finance.FnPayment.MasterPayment
{
    public class GetListEventPaymentRequest : ISearch
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string Search { get; set; }
        public IEnumerable<string> SearchBy { get; set; }

        private string _searchPattern;
        public string SearchPattern()
        {
            return _searchPattern ??= !string.IsNullOrWhiteSpace(Search) ? $"%{Search}%" : "%";
        }
    }
}
