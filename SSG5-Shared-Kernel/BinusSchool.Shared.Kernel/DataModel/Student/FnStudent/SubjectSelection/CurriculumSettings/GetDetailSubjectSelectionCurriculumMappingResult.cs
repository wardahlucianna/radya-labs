using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings
{
    public class GetDetailSubjectSelectionCurriculumMappingResult
    {
        public ItemValueVm SubjectSelectionCurriculum { get; set; }
        public string Description { get; set; }
        public List<GetDetailSubjectSelectionCurriculumMappingResult_CurriculumGrade> CurriculumGrades { get; set; }
        public int? MinSubjectSelection { get; set; }
        public int? MaxSubjectSelection { get; set; }
        public List<GetDetailSubjectSelectionCurriculumMappingResult_SubjectLevel> SubjectLevels { get; set; }
        public bool CanUpdate { get; set; }
    }

    public class GetDetailSubjectSelectionCurriculumMappingResult_CurriculumGrade
    {
        public ItemValueVm Grade { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class GetDetailSubjectSelectionCurriculumMappingResult_SubjectLevel
    {
        public ItemValueVm SubjectLevel { get; set; }
        public int? MinRange { get; set; }
        public int? MaxRange { get; set; }
    }
}