using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.ClearanceForm
{
    public class GetAllStudentStatusClearanceFormResult
    {
        public bool IsAnySubmitted { get; set; }
        public List<AllStudentStatusClearanceForm_Student> ListChild { get; set; }
    }

    public class AllStudentStatusClearanceForm_Student
    {
        public bool IsSubmitted { get; set; }
        public NameValueVm Student { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm BLPStatus { get; set; }
        public ItemValueVm Group { get; set; }
        public StudentColorDescription FinalStatus { get; set; }
        public StudentColorDescription ConsentForm { get; set; }
        public StudentColorDescription ClearanceForm { get; set; }
        public StudentColorDescription MedicalClearance { get; set; }
    }

    public class StudentColorDescription
    {
        public BLPFinalStatus StatusEnum { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
    }
}
