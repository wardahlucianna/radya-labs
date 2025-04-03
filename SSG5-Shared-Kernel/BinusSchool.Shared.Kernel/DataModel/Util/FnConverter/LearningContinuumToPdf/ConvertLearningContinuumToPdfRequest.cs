using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Util.FnConverter.LearningContinuumToPdf
{
    public class ConvertLearningContinuumToPdfRequest
    {
        public string IdSchool { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm SubjectContinuum { get; set; }
        public ItemValueVm Student { get; set; }
        public ExportPdfLearningContinuumToPdfRequest_Entry Entry { get; set; }
        public ExportPdfLearningContinuumToPdfRequest_Summary Summary { get; set; } 
    }
    public class ExportPdfLearningContinuumToPdfRequest_Entry
    {
        public string IdUser { get; set; }
        public string? IdClass { get; set; }
        public string? IdHomeroom { get; set; }
    }
    public class ExportPdfLearningContinuumToPdfRequest_Summary
    {
        public string IdFilterAcademicYear { get; set; }
        public int FilterSemester { get; set; }
        public string IdFilterLevel { get; set; }
        public string IdFilterHomeroom { get; set; }
        public string IdFilterGrade { get; set; }
        public string IdFilterSubjectContinuum { get; set; }
    }
}
