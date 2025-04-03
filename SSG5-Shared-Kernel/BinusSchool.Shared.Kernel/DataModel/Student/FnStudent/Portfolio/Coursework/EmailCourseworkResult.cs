using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework
{
    public class EmailCourseworkResult
    {
        public string Id { get; set; }
        public CodeWithIdVm UOI { get; set; }
        public string Content { get; set; }
        public string IdUser { get; set; }
        public string TeacherName { get; set; }
        public List<DataParentCourseworkAnecdotal> DataParents { get; set; }
        public string IdStudent { get; set; }
        public string SchoolName { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public DateTime Date { get; set; }
        public bool NotifyParentStudent { get; set; }
        public int Type { get; set; }
        public bool ForStudent { get; set; }
    }
    public class DataParentCourseworkAnecdotal
    {
        public string ParentName { get; set; }
        public string EmailParent { get; set; }
    }
}
