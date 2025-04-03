using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class UpdateTimetableRequest
    {
        public string Id { get; set; }
        public bool IsActive { get; set; }
    }
}
