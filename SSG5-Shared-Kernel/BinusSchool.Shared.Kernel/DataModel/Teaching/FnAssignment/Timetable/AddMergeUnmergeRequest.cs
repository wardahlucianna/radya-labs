using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class AddMergeUnmergeRequest
    {
        public string Id { get; set; }
        public bool IsMarge { get; set; }
        public List<string> ChildId { get; set; }
    }
}
