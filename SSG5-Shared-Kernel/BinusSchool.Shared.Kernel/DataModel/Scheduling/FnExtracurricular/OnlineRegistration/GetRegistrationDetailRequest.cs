using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetRegistrationDetailRequest : CollectionRequest
    {
        public List<string> IdStudent { get; set; }
    }
}
