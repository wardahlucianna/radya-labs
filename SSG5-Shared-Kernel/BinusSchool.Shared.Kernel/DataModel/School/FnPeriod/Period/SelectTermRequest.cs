﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnPeriod.Period
{
    public class SelectTermRequest : CollectionRequest
    {
        public IEnumerable<string> IdGrade { get; set; }
    }
}
