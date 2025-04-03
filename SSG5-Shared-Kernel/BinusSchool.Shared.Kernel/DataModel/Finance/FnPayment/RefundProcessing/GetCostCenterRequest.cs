﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class GetCostCenterRequest : ISearch
    {
        public string IdSchool { get; set; }
        public string Search { get; set; }
        public IEnumerable<string> SearchBy { get; set; }

        private string _searchPattern;
        public string SearchPattern()
        {
            return _searchPattern ??= !string.IsNullOrWhiteSpace(Search) ? $"%{Search}%" : "%";
        }
    }
}
