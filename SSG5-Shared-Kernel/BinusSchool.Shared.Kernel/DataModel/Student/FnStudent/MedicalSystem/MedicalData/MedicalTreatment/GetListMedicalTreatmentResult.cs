using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment
{
    public class GetListMedicalTreatmentResult : IItemValueVm
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool CanDelete { get; set; }
    }
}
