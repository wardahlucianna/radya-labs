using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.OccupationType
{
    public class GetOccupationTypeRequest : CollectionRequest
    {
        public string IdOccupationType { get; set; }
    }
}
