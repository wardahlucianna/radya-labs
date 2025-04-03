using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryStaffListRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public int? IdDesignation { get; set; }
    }
}
