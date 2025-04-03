using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ParentRelationship
{
    public class GetParentRelationshipRequest : CollectionRequest
    {
        public int IdParentRelationship { get; set; }
    }
}
