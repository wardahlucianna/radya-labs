using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentGenderDemographyDetailResult
    {
        public NameValueVm Student { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public NameValueVm Teacher { get; set; }
        public ItemValueVm Streaming { get; set; }
        public Gender Gender { get; set; }
    }
}
