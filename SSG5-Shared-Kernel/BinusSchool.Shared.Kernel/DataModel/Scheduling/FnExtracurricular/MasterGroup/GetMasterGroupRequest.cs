using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup
{
    public class GetMasterGroupRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public bool? Status { get; set; }
    }
}
