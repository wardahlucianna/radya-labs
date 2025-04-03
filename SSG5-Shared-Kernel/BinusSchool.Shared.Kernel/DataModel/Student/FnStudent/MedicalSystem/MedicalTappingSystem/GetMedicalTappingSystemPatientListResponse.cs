using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem
{
    public class GetMedicalTappingSystemPatientListResponse
    {
        public ItemValueVm IdBinusian { get; set; }
        public string Name { get; set; }
        public CodeWithIdVm SchoolLevel { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public bool IsCheckedIn { get; set; }
        public string Mode { get; set; }
    }
}
