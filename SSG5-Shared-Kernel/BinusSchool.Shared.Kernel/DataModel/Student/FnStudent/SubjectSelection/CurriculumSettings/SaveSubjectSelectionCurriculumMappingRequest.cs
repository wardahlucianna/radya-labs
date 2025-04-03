using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings
{
    public class SaveSubjectSelectionCurriculumMappingRequest
    {
        public string CurriculumGroup { get; set; }
        public string IdSubjectSelectionCurriculum { get; set; }
        public string Description { get; set; }
        public List<string> IdGrade { get; set; }
        public int? MinSubjectSelection { get; set; }
        public int? MaxSubjectSelection { get; set; }
        public List<SaveSubjectSelectionCurriculumMappingRequest_SubjectLevel> SubjectLevels { get; set; }
    }

    public class SaveSubjectSelectionCurriculumMappingRequest_SubjectLevel
    {
        public string IdSubjectLevel { get; set; }
        public int? MinRange { get; set; }
        public int? MaxRange { get; set; }
    }
}
