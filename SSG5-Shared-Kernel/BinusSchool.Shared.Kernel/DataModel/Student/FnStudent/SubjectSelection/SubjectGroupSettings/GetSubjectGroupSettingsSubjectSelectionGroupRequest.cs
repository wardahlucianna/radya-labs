using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings
{
    public class GetSubjectGroupSettingsSubjectSelectionGroupRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
