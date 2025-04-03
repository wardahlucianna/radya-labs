using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings
{
    public class GetSubjectGroupSettingsCurriculumMappingResponse
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Curriculum { get; set; }
        public List<GetSubjectGroupSettingsCurriculumMappingResponse_MappingSubjectSelectionGroup> MappingSubjectSelectionGroups { get; set; }
        public bool ActiveStatus { get; set; }
    }

    public class GetSubjectGroupSettingsCurriculumMappingResponse_MappingSubjectSelectionGroup
    {
        public string IdMappingSubjectSelectionGroup { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
