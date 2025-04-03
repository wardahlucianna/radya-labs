using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListLearningOutcomeResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public int Order { get; set; }
    }
}
