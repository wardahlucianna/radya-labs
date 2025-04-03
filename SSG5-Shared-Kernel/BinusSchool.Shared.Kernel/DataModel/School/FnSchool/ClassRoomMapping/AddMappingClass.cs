using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping
{
    public class AddMappingClass
    {
        public string IdGrade { get; set; }
        public List<string> Pathways { get; set; }
        public List<AddClassMappingDivision> Classrooms { get; set; }
    }

    public class AddClassMappingDivision 
    {
        public string IdClassroom { get; set; }
        public List<string> IdDivision { get; set; }
    }
}
