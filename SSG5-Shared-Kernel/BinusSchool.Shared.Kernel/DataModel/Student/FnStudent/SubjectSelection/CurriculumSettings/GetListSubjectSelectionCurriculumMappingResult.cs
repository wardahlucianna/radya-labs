using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings
{
    public class GetListSubjectSelectionCurriculumMappingResult
    {
        public string CurriculumGroup { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Curriculum { get; set; }
        public int? MinSubjectSelection { get; set; }
        public int? MaxSubjectSelection { get; set; }
        public string CurriculumGrades { get; set; }
        public bool CanDelete { get; set; }
    }
}
