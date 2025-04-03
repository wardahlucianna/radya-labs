using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem
{
    public class GetDetailMedicalItemResult : IItemValueVm
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public ItemValueVm MedicalItemType { get; set; }
        public bool IsCommonDrug { get; set; }
        public ItemValueVm MedicineType { get; set; }
        public ItemValueVm MedicineCategory { get; set; }
        public CodeWithIdVm DosageType { get; set; }
    }
}
