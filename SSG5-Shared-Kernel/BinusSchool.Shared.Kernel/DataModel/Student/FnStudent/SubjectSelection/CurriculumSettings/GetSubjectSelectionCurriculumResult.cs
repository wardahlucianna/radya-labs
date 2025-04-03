using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings
{
    public class GetSubjectSelectionCurriculumResult
    {
        public string IdSubjectSelectionCurriculum { get; set; }
        public string CurriculumName { get; set; }
        public bool CanDelete { get; set; }
    }
}
