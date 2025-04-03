using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem
{
    public class GetListMedicalItemResult : IItemValueVm
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string MedicalItemType { get; set; }
        public bool IsCommonDrug { get; set; }
        public string DosageType { get; set; }
        public bool CanDelete { get; set; }
    }
}
