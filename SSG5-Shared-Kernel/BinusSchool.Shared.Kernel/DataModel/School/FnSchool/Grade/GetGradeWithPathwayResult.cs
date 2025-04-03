using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeWithPathwayResult : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public string Pathway => string.Join(", ", Pathways.Select(x => x.Description));
        public IEnumerable<ItemValueVm> Pathways { get; set; }
    }
}
