using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerUniversityPortalRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
