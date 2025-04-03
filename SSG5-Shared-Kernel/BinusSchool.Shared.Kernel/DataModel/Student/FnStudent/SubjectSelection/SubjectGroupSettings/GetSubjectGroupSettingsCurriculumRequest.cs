using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings
{
    public class GetSubjectGroupSettingsCurriculumRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdGrade { get; set; }
    }
}
