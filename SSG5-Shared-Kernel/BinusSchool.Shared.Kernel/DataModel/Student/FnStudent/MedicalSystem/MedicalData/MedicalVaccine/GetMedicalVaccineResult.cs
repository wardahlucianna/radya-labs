using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine
{
    public abstract class GetMedicalVaccineResult : IItemValueVm
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public ItemValueVm DosageType { get; set; }
        public int DosageAmount { get; set; }
    }

    public class GetListMedicalVaccineResult : GetMedicalVaccineResult
    {
        public bool CanDelete { get; set; }
    }

    public class GetDetailMedicalVaccineResult : GetMedicalVaccineResult { }
}
