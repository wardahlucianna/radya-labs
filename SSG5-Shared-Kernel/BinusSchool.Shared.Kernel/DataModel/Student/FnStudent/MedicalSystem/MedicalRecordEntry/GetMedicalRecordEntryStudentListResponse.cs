using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryStudentListResponse
    {
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string Name { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }
}
