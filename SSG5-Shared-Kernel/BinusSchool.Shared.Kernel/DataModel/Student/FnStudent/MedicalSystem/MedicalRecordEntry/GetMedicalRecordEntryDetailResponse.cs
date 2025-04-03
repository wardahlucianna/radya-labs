using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryDetailResponse
    {
        public ItemValueVm IdBinusian { get; set; }
        public string Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Age { get; set; }
        public GetMedicalRecordEntryDetailResponse_ClinicVisit ClinicVisitation { get; set; }
        public string ImageUrl { get; set; }
        public string Gender { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }

    public class GetMedicalRecordEntryDetailResponse_ClinicVisit
    {
        public int RunningMonth { get; set; }
        public int RunningAccumulative { get; set; }
    }
}
