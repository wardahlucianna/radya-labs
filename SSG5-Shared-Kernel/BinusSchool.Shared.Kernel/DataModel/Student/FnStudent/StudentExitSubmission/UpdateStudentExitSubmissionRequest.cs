using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitSubmission
{
    public class UpdateStudentExitSubmissionRequest
    {
        public string Id { get; set; }
        public StatusExitStudent Status { get; set; }
        public string Note { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
}
