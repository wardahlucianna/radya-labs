using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework
{
    public class GetDetailCourseworkResult
    {
        public string Id { get; set; }
        public CodeWithIdVm UOI { get; set; }
        public string Content { get; set; }
        public bool NotifyParentStudent { get; set; }
        public int Type { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public List<CourseworkAttachment> Attachments { get; set; }
    }
}
