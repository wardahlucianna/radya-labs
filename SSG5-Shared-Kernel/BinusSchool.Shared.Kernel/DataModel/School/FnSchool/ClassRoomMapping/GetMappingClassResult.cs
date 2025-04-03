using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping
{
    public class GetMappingClassResult : IItemValueVm
    {
        public string Id { get; set; }
        public string Grade { get; set; }
        public string Pathway { get; set; }
        public string Classroom { get; set; }
        public string Description { get; set; }
    }
}
