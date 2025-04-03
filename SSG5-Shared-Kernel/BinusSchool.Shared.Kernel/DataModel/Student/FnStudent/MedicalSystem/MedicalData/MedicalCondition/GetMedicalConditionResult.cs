using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition
{
    public abstract class GetMedicalConditionResult : IItemValueVm
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<string> MedicatedWith { get; set; }
        public List<string> TreatedWith { get; set; }
    }

    public class GetListMedicalConditionResult : GetMedicalConditionResult
    {
        public bool CanDelete { get; set; }
    }

    public class GetDetailMedicalConditionResult : GetMedicalConditionResult
    {
    }
}
