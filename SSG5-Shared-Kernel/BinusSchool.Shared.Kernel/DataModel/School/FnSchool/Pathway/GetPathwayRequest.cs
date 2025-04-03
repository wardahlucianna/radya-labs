using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.Pathway
{
    public class GetPathwayRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public string IdPathway { get; set; }
    }
}
