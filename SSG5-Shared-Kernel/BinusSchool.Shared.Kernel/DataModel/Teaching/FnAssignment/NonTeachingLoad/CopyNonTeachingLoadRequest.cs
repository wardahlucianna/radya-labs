using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class CopyNonTeachingLoadRequest
    {
        public string IdAcadyearTarget { get; set; }
        public List<string> Ids { get; set; } = new List<string>();
    }
}
