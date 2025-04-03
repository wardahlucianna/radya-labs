using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryStaffListResponse
    {
        public ItemValueVm IdBinusian { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
