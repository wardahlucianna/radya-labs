using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem
{
    public class UpdateMedicalTappingSystemPatientStatusResponse
    {
        public string IdBinusian { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public CodeWithIdVm SchoolLevel { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
}
