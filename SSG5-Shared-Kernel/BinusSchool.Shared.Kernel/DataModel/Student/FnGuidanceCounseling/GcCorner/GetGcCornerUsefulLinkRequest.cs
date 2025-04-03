using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerUsefulLinkRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string IdRoleGroup { get; set; }
    }
}
