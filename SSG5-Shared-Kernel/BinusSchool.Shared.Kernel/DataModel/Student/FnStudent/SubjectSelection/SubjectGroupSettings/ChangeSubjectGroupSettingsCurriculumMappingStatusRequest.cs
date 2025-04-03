using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings
{
    public class ChangeSubjectGroupSettingsCurriculumMappingStatusRequest
    {
        public List<string> IdMappingCurriculumSubjectGroups { get; set; }
        public bool ActiveStatus { get; set; }
    }
}
