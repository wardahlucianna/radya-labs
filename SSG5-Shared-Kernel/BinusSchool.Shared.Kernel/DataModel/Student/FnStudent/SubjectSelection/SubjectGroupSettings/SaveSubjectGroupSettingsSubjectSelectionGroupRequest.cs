using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings
{
    public class SaveSubjectGroupSettingsSubjectSelectionGroupRequest
    {
        public string IdSubjectSelectionGroup { get; set; }
        public string IdSchool { get; set; }
        public string Description { get; set; }
    }
}
