using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping
{
    public class GetMappingClassDetailResult
    {
        public string Id { get; set; }
        public CodeWithIdVm Acadyear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public List<PathwayDetail> Pathways { get; set; }
        public List<PathwayClassroom> Classrooms { get; set; }
    }

    public class PathwayDetail : CodeWithIdVm
    {
        public string IdPathwayDetail { get; set; }
        public string IdGradePathway { get; set; }
        public bool IsAlreadyUse { get; set; }
    }

    public class PathwayClassroom : CodeWithIdVm
    {
        public string IdPathwayClassroom { get; set; }
        public string IdGradePathway { get; set; }
        public List<ClassroomDivision> Divisions { get; set; }
        public bool IsAlreadyUse { get; set; }
    }

    public class ClassroomDivision : CodeWithIdVm
    {
        public string IdClassroomDivision { get; set; }
        public string IdPathwayClassroom { get; set; }
        public bool IsAlreadyUse { get; set; }
    }
}
